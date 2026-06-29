# Tổng Kết Kiến Thức và Rút Kinh Nghiệm - Giai đoạn 2 (Task 2.1 & 2.2)

Tài liệu ghi nhận bối cảnh, các lỗi phát sinh, giải pháp xử lý và kiến thức tích lũy sau khi hoàn thành thiết lập gửi lệnh điều khiển MAVLink và nâng cấp giao diện GCS Dark Mode.

---

## 1. Thông tin chung
*   **Giai đoạn / Task hoàn thành:** Giai đoạn 2 (Task 2.1 & 2.2) - Điều khiển Drone & Nâng cấp UI/UX.
*   **Ngày hoàn thành:** 15/06/2026

---

## 2. Các vấn đề kỹ thuật (Bugs/Issues) đã gặp phải

### 1. Vấn đề 1: Trùng lặp/nhầm lẫn SysID và CompID của Drone
*   **Mô tả lỗi:** 
    Khi gửi lệnh điều khiển xuống Drone (SITL), Drone bỏ qua lệnh (không ARM, không đổi mode) và báo lỗi không khớp ID hoặc không ghi nhận lệnh.
*   **Nguyên nhân gốc (Root Cause):** 
    GCS gửi các gói tin lệnh `COMMAND_LONG` với `target_system = 1` và `target_component = 1` là giá trị tĩnh (hardcoded). Nếu drone giả lập chạy trên một cấu hình ID khác (ví dụ: `sysid=2`, `compid=1`), lệnh sẽ bị bỏ qua.
*   **Giải pháp xử lý:** 
    Trong [MavlinkService.cs](file:///d:/Training_Framework_Project%20UAV%20GCS%20giao%20hang%20Cuu%20tro/UAV_giaohangcuutro/Core/Mavlink/MavlinkService.cs), tạo biến thành viên động để tự động capture `_droneSysId = packet.sysid` và `_droneCompId = packet.compid` từ bất kỳ gói telemetry nào gửi tới. Khi gửi lệnh, điền các giá trị dynamic này vào cấu trúc gói gửi đi.

### 2. Vấn đề 2: Lỗi không thể gửi lệnh do thiếu Endpoint đích trong UDP
*   **Mô tả lỗi:**
    UDP hoạt động theo cơ chế không kết nối (Connectionless). Khi gọi hàm gửi lệnh, socket báo lỗi hoặc lệnh không được gửi đi vì socket không biết địa chỉ đích gửi tới là gì.
*   **Nguyên nhân gốc (Root Cause):**
    Code UDP ban đầu chỉ bind để lắng nghe (`IPAddress.Loopback` trên port 14551) chứ không cấu hình địa chỉ đích (Remote IP và Remote Port) để gửi gói tin đi.
*   **Giải pháp xử lý:**
    Trong [UdpConnection.cs](file:///d:/Training_Framework_Project%20UAV%20GCS%20giao%20hang%20Cuu%20tro/UAV_giaohangcuutro/Core/Connection/UdpConnection.cs), bắt giữ Endpoint gửi gói tin gần nhất của Drone thông qua `result.RemoteEndPoint` thu được từ hàm `ReceiveAsync()`, lưu vào biến `_lastRemoteEP`. Khi gửi lệnh đi, sử dụng nạp chồng `_udpClient.Send(data, data.Length, _lastRemoteEP)`.

---

## 3. Kiến thức & Kỹ năng cốt lõi đã học được

### Về C# / Lập trình hệ thống:
*   **Thiết kế UI/UX Programmatic trong WinForms:** Tự khởi tạo và sắp xếp các control (`Panel`, `TableLayoutPanel`, `NumericUpDown`, `Button`) hoàn toàn bằng code, giúp thiết kế layout co giãn tự động linh hoạt mà không bị phụ thuộc vào file Designer của Visual Studio.
*   **Styling Custom Dark Mode:** Sử dụng cách vẽ thủ công đường viền (Border Paint event) và các màu Charcoal/Slate đặc thù (`Color.FromArgb`) giúp tạo ra giao diện tối cao cấp vượt trội so với các phong cách WinForms mặc định của Windows.
*   **Đồng bộ đa luồng an toàn (BeginInvoke):** 8 thẻ telemetry cập nhật tần số cao (khoảng 5Hz-10Hz) từ các sự kiện bất đồng bộ của MAVLink mà hoàn toàn không xảy ra xung đột luồng hoặc làm đơ giao diện.

### Về Giao thức (MAVLink):
*   **Gửi lệnh điều khiển (`COMMAND_LONG`):** Cách chuẩn bị gói tin điều khiển dùng struct `mavlink_command_long_t`, chỉ định mã lệnh `command` và nạp tham số từ `param1` đến `param7`.
    *   *ARM:* Lệnh `COMPONENT_ARM_DISARM` (400), `param1 = 1.0f` (ARM) / `0.0f` (DISARM).
    *   *TAKEOFF:* Lệnh `TAKEOFF` (22), `param7 = độ cao tính bằng mét`.
*   **Chuyển chế độ bay (`SET_MODE`):** Hiểu rằng để chuyển chế độ bay (ví dụ sang `GUIDED`), ta cần gửi gói tin `SET_MODE` (Message ID #11) và chuyển giá trị mode custom sang dạng số nguyên (GUIDED trong ArduCopter là `4`).
*   **Luật an toàn bay của FC (Flight Controller):** Hiểu được nguyên tắc vận hành: Drone bắt buộc phải được chuyển sang chế độ `GUIDED` và được `ARM` thành công trước khi gửi lệnh `TAKEOFF`.

### Về quy trình làm với AI:
*   **Lập kế hoạch trước khi code (Planning Mode):** Xác định rõ các lớp cần sửa đổi và luồng uplink/downlink dữ liệu trước khi sinh code, giúp giảm thiểu 90% lỗi thiết kế.

---

## 4. Danh sách các Lệnh MAVLink / Cấu trúc dữ liệu đã sử dụng

| ID gói tin | Tên cấu trúc MAVLink | Chức năng & Tham số cấu hình |
| :--- | :--- | :--- |
| **`SET_MODE` (#11)** | `MAVLink.mavlink_set_mode_t` | Thay đổi Flight Mode của Drone. GCS truyền `base_mode = MAV_MODE_FLAG.CUSTOM_MODE_ENABLED` (1) và `custom_mode = 4` (GUIDED). |
| **`COMMAND_LONG` (#76)** | `MAVLink.mavlink_command_long_t` | Đóng gói và gửi các lệnh điều khiển dài xuống FC. Sử dụng: <br>- Lệnh **400** (`COMPONENT_ARM_DISARM`) để ARM/DISARM.<br>- Lệnh **22** (`TAKEOFF`) để bay lên độ cao đặt trước. |

---

## 5. Tự đánh giá & Kế hoạch tiếp theo
*   **Mức độ hiểu bài:** [x] Rất tự tin
*   **Những rủi ro tiềm ẩn (Technical Debt) để lại cho Task sau:**
    *   Hiện tại, việc chuyển đổi chế độ bay và lệnh ARM/Takeoff là độc lập. Ở Giai đoạn 3 và 4, cần quản lý trạng thái máy bay chặt chẽ hơn (tránh trường hợp người dùng bấm Takeoff khi chưa ARM hoặc chưa chuyển sang GUIDED).
    *   Chưa có phản hồi xác thực lệnh (`COMMAND_ACK` từ Drone). FC gửi lại gói tin `COMMAND_ACK` (#77) để xác nhận lệnh thực thi thành công hay thất bại. Cần lắng nghe gói tin này để hiển thị thông báo lỗi lên UI nếu lệnh bị Drone từ chối.
