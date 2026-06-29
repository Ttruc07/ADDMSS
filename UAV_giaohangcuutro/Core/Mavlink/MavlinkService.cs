using System;
using System.IO;
using UAV_giaohangcuutro.Core.Connection;

namespace UAV_giaohangcuutro.Core.Mavlink
{
    public class MavlinkService : IDisposable
    {
        private readonly UdpConnection _connection;
        private readonly MAVLink.MavlinkParse _parser;
        private byte _droneSysId = 1;
        private byte _droneCompId = 1;
        private bool _isMavlink2 = false;
        private System.Threading.Timer? _heartbeatTimer;

        public event Action<MAVLink.mavlink_heartbeat_t>? OnHeartbeatReceived;
        public event Action<MAVLink.mavlink_attitude_t>? OnAttitudeReceived;
        public event Action<MAVLink.mavlink_sys_status_t>? OnSysStatusReceived;
        public event Action<MAVLink.mavlink_global_position_int_t>? OnGlobalPositionReceived;
        public event Action<MAVLink.mavlink_mission_request_t>? OnMissionRequestReceived;
        public event Action<MAVLink.mavlink_mission_request_int_t>? OnMissionRequestIntReceived;
        public event Action<MAVLink.mavlink_mission_ack_t>? OnMissionAckReceived;
        public event Action<MAVLink.mavlink_gps_raw_int_t>? OnGpsRawReceived;
        public event Action<string>? OnLogMessage;

        public MavlinkService(UdpConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _parser = new MAVLink.MavlinkParse();
            _connection.OnDataReceived += HandleDataReceived;
        }

        public void StartHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new System.Threading.Timer(SendHeartbeatCallback, null, 0, 1000);
            OnLogMessage?.Invoke("GCS Heartbeat sender started (1Hz).");
        }

        public void StopHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            OnLogMessage?.Invoke("GCS Heartbeat sender stopped.");
        }

        private void SendHeartbeatCallback(object? state)
        {
            try
            {
                var msg = new MAVLink.mavlink_heartbeat_t
                {
                    type = (byte)MAVLink.MAV_TYPE.GCS,
                    autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                    base_mode = 0,
                    custom_mode = 0,
                    system_status = (byte)MAVLink.MAV_STATE.ACTIVE,
                    mavlink_version = 3
                };
                SendPacket(MAVLink.MAVLINK_MSG_ID.HEARTBEAT, msg);
            }
            catch
            {
                // Ignore background heartbeat exceptions
            }
        }

        private void HandleDataReceived(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    while (ms.Position < ms.Length)
                    {
                        var packet = _parser.ReadPacket(ms);
                        if (packet == null)
                        {
                            // If parsing failed (incomplete packet or bad CRC), stop processing this buffer
                            break;
                        }

                        ProcessMavlinkMessage(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"MavlinkService parsing error: {ex.Message}");
            }
        }

        private void ProcessMavlinkMessage(MAVLink.MAVLinkMessage packet)
        {
            try
            {
                switch (packet.msgid)
                {
                    case (uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT:
                        var heartbeat = packet.ToStructure<MAVLink.mavlink_heartbeat_t>();
                        
                        // Auto-detect MAVLink version from drone packets
                        _isMavlink2 = packet.ismavlink2;
                        
                        // Only capture drone IDs from drone heartbeats (avoid GCS loopback override)
                        if (packet.sysid != 255)
                        {
                            if (_droneSysId != packet.sysid || _droneCompId != packet.compid)
                            {
                                _droneSysId = packet.sysid;
                                _droneCompId = packet.compid;
                                OnLogMessage?.Invoke($"[HEARTBEAT] Locked onto drone SysID: {_droneSysId}, CompID: {_droneCompId} (MAVLink {(_isMavlink2 ? "2.0" : "1.0")})");
                            }
                        }
                        
                        OnHeartbeatReceived?.Invoke(heartbeat);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.ATTITUDE:
                        var attitude = packet.ToStructure<MAVLink.mavlink_attitude_t>();
                        OnAttitudeReceived?.Invoke(attitude);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.SYS_STATUS:
                        var sysStatus = packet.ToStructure<MAVLink.mavlink_sys_status_t>();
                        OnSysStatusReceived?.Invoke(sysStatus);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT:
                        var globalPos = packet.ToStructure<MAVLink.mavlink_global_position_int_t>();
                        OnGlobalPositionReceived?.Invoke(globalPos);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.MISSION_REQUEST:
                        var missionReq = packet.ToStructure<MAVLink.mavlink_mission_request_t>();
                        OnMissionRequestReceived?.Invoke(missionReq);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.MISSION_REQUEST_INT:
                        var missionReqInt = packet.ToStructure<MAVLink.mavlink_mission_request_int_t>();
                        OnMissionRequestIntReceived?.Invoke(missionReqInt);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.MISSION_ACK:
                        var missionAck = packet.ToStructure<MAVLink.mavlink_mission_ack_t>();
                        OnMissionAckReceived?.Invoke(missionAck);
                        break;

                    case (uint)MAVLink.MAVLINK_MSG_ID.GPS_RAW_INT:
                        var gpsRaw = packet.ToStructure<MAVLink.mavlink_gps_raw_int_t>();
                        OnGpsRawReceived?.Invoke(gpsRaw);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Error processing MAVLink packet ID {packet.msgid}: {ex.Message}");
            }
        }

        public void SendPacket(MAVLink.MAVLINK_MSG_ID msgId, object msgData)
        {
            try
            {
                byte[] packet;
                if (_isMavlink2)
                {
                    packet = _parser.GenerateMAVLinkPacket20(msgId, msgData, false, 255, (byte)MAVLink.MAV_COMPONENT.MAV_COMP_ID_MISSIONPLANNER);
                }
                else
                {
                    packet = _parser.GenerateMAVLinkPacket10(msgId, msgData, 255, (byte)MAVLink.MAV_COMPONENT.MAV_COMP_ID_MISSIONPLANNER);
                }
                _connection.SendData(packet);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Failed to send packet {msgId}: {ex.Message}");
            }
        }

        public void SetMode(uint customMode)
        {
            var msg = new MAVLink.mavlink_set_mode_t
            {
                target_system = _droneSysId,
                base_mode = (byte)MAVLink.MAV_MODE_FLAG.CUSTOM_MODE_ENABLED,
                custom_mode = customMode
            };
            OnLogMessage?.Invoke($"Sending SetMode command: Mode {customMode} (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) for drone SysID: {_droneSysId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.SET_MODE, msg);
        }

        public void ArmDisarm(bool arm)
        {
            var msg = new MAVLink.mavlink_command_long_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                command = (ushort)MAVLink.MAV_CMD.COMPONENT_ARM_DISARM,
                confirmation = 0,
                param1 = arm ? 1.0f : 0.0f, // 1 to arm, 0 to disarm
                param2 = 0,
                param3 = 0,
                param4 = 0,
                param5 = 0,
                param6 = 0,
                param7 = 0
            };
            OnLogMessage?.Invoke($"Sending {(arm ? "ARM" : "DISARM")} command (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msg);
        }

        public void Takeoff(float altitudeMeters)
        {
            var msg = new MAVLink.mavlink_command_long_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                command = (ushort)MAVLink.MAV_CMD.TAKEOFF,
                confirmation = 0,
                param1 = 0,
                param2 = 0,
                param3 = 0,
                param4 = 0,
                param5 = 0,
                param6 = 0,
                param7 = altitudeMeters // Takeoff altitude in meters
            };
            OnLogMessage?.Invoke($"Sending TAKEOFF command to {altitudeMeters}m (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msg);
        }

        public void StartMission()
        {
            var msg = new MAVLink.mavlink_command_long_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                command = (ushort)MAVLink.MAV_CMD.MISSION_START,
                confirmation = 0,
                param1 = 0,
                param2 = 0,
                param3 = 0,
                param4 = 0,
                param5 = 0,
                param6 = 0,
                param7 = 0
            };
            OnLogMessage?.Invoke($"Sending MISSION_START command (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msg);
        }

        public void SetPositionTarget(double lat, double lon, float altitudeMeters)
        {
            var msg = new MAVLink.mavlink_set_position_target_global_int_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                coordinate_frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, // 6: relative altitude in meters
                type_mask = 2040, // 0x07F8: ignore velocity & accel, only command Position Lat/Lon/Alt
                lat_int = (int)(lat * 1E7),
                lon_int = (int)(lon * 1E7),
                alt = altitudeMeters,
                vx = 0, vy = 0, vz = 0,
                afx = 0, afy = 0, afz = 0,
                yaw = 0, yaw_rate = 0
            };
            OnLogMessage?.Invoke($"Sending SetPositionTarget: Lat {lat:F6}, Lon {lon:F6}, Alt {altitudeMeters}m (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.SET_POSITION_TARGET_GLOBAL_INT, msg);
        }

        public void SendMissionCount(ushort count)
        {
            var msg = new MAVLink.mavlink_mission_count_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                count = count,
                mission_type = 0 // MAV_MISSION_TYPE_MISSION
            };
            OnLogMessage?.Invoke($"[MISSION] Sending MISSION_COUNT: {count} items (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.MISSION_COUNT, msg);
        }

        public void SendMissionItem(MAVLink.mavlink_mission_item_t item)
        {
            // Update item targets just in case
            item.target_system = _droneSysId;
            item.target_component = _droneCompId;
            
            OnLogMessage?.Invoke($"[MISSION] Sending MISSION_ITEM {item.seq} (Cmd: {item.command}) (MAVLink {(_isMavlink2 ? "2.0" : "1.0")})...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.MISSION_ITEM, item);
        }

        public void SendMissionItemInt(MAVLink.mavlink_mission_item_t item)
        {
            var itemInt = new MAVLink.mavlink_mission_item_int_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                seq = item.seq,
                frame = item.frame,
                command = item.command,
                current = item.current,
                autocontinue = item.autocontinue,
                param1 = item.param1,
                param2 = item.param2,
                param3 = item.param3,
                param4 = item.param4,
                x = IsGlobalFrame(item.frame) ? (int)Math.Round(item.x * 1e7) : (int)item.x,
                y = IsGlobalFrame(item.frame) ? (int)Math.Round(item.y * 1e7) : (int)item.y,
                z = item.z,
                mission_type = item.mission_type
            };

            // Map standard global frame to INT version for MISSION_ITEM_INT structure
            if (itemInt.frame == (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT)
            {
                itemInt.frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT;
            }
            else if (itemInt.frame == (byte)MAVLink.MAV_FRAME.GLOBAL)
            {
                itemInt.frame = (byte)MAVLink.MAV_FRAME.GLOBAL_INT;
            }

            OnLogMessage?.Invoke($"[MISSION] Sending MISSION_ITEM_INT {itemInt.seq} (Cmd: {itemInt.command}) (MAVLink {(_isMavlink2 ? "2.0" : "1.0")})...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.MISSION_ITEM_INT, itemInt);
        }

        private bool IsGlobalFrame(byte frame)
        {
            return frame == (byte)MAVLink.MAV_FRAME.GLOBAL ||
                   frame == (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT ||
                   frame == (byte)MAVLink.MAV_FRAME.GLOBAL_INT ||
                   frame == (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT ||
                   frame == 10 || // MAV_FRAME_GLOBAL_TERRAIN_ALT
                   frame == 11;   // MAV_FRAME_GLOBAL_TERRAIN_ALT_INT
        }

        public void ClearMission()
        {
            var msg = new MAVLink.mavlink_mission_clear_all_t
            {
                target_system = _droneSysId,
                target_component = _droneCompId,
                mission_type = 0
            };
            OnLogMessage?.Invoke($"[MISSION] Sending MISSION_CLEAR_ALL (MAVLink {(_isMavlink2 ? "2.0" : "1.0")}) to drone SysID: {_droneSysId}, CompID: {_droneCompId}...");
            SendPacket(MAVLink.MAVLINK_MSG_ID.MISSION_CLEAR_ALL, msg);
        }

        public void Dispose()
        {
            _connection.OnDataReceived -= HandleDataReceived;
        }
    }
}
