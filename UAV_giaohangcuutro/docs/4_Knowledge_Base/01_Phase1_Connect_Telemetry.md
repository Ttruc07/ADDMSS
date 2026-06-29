# Tổng Kết Kiến Thức và Rút Kinh Nghiệm - Giai đoạn 1 (Task 1.1)

Tài liệu ghi nhận bối cảnh dự án, các lỗi phát sinh, giải pháp xử lý và kiến thức tích lũy sau khi hoàn thành kết nối UDP và đọc Telemetry thô từ drone/SITL.

---

## 1. Thông tin chung
*   **Giai đoạn / Task hoàn thành:** Giai đoạn 1 (Task 1.1) - Kết nối UDP & Đọc Telemetry thô.
*   **Ngày hoàn thành:** 15/06/2026

---

## 2. Các vấn đề kỹ thuật (Bugs/Issues) đã gặp phải

### 1. Vấn đề 1: Lỗi chiếm dụng Socket (Port in use)
*   **Mô tả lỗi:** 
    Khi khởi chạy ứng dụng hoặc chạy thử nghiệm lần thứ hai, hệ thống báo lỗi:
    `Failed to start UDP connection: Only one usage of each socket address (protocol/network address/port) is normally permitted.`
*   **Nguyên nhân gốc (Root Cause):** 
    UDP Socket sau khi đóng hoặc khi ứng dụng bị tắt đột ngột vẫn giữ cổng ở trạng thái chờ giải phóng (`TIME_WAIT`). Hoặc do có tiến trình khác đang lắng nghe cùng một cổng và địa chỉ IP đó.
*   **Giải pháp xử lý:** 
    Sử dụng thuộc tính `ReuseAddress` trước khi thực hiện liên kết (bind) cổng. Trong C#:
    ```csharp
    _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    ```

### 2. Vấn đề 2: Không nhận được dữ liệu Telemetry mặc dù SITL/Mission Planner đang chạy
*   **Mô tả lỗi:**
    Ứng dụng khởi động thành công không báo lỗi nhưng không ghi nhận được bất kỳ byte dữ liệu nào từ cổng UDP `14551`. Trong khi đó, script Python mẫu vẫn nhận thành công qua `udp:127.0.0.1:14551`.
*   **Nguyên nhân gốc (Root Cause):**
    Code C# ban đầu thực hiện liên kết với `IPAddress.Any` (`0.0.0.0`). Trong môi trường Windows, Windows Firewall có thể chặn dữ liệu Loopback/Localhost chuyển tiếp từ Mission Planner khi bind vào `Any`, hoặc định tuyến socket cục bộ không khớp chính xác với cổng forwarding (Target output của Mission Planner là `127.0.0.1:14551`).
*   **Giải pháp xử lý:**
    Chuyển đổi từ liên kết mọi địa chỉ (`IPAddress.Any`) sang liên kết cụ thể với địa chỉ Loopback cục bộ (`IPAddress.Loopback` - `127.0.0.1`):
    ```csharp
    _udpClient.Client.Bind(new IPEndPoint(IPAddress.Loopback, port));
    ```

---

## 3. Kiến thức & Kỹ năng cốt lõi đã học được

### Về C# / Lập trình hệ thống:
*   **Xử lý bất đồng bộ (Asynchronous Loop):** Sử dụng `Task.Run` chạy nhận dữ liệu UDP dưới nền thông qua `UdpClient.ReceiveAsync(CancellationToken)` giúp giao diện (UI Thread) hoàn toàn mượt mà, không bị treo.
*   **Thread-safe UI Updates (Cập nhật giao diện an toàn đa luồng):**
    Dữ liệu nhận từ UDP được xử lý ở một background thread khác. Do đó, việc cập nhật trực tiếp lên `RichTextBox` ở Form chính phải thông qua cơ chế kiểm tra `InvokeRequired` và gọi `BeginInvoke` để tránh lỗi `Cross-thread operation not valid`.
*   **Kiểm soát rác bộ nhớ (Memory Management):**
    Thêm đoạn code giới hạn số lượng dòng hiển thị của `RichTextBox` tối đa 500 dòng và cắt bớt dòng cũ để tránh ứng dụng bị phình bộ nhớ (Out Of Memory) khi bay thực tế trong thời gian dài.

### Về Giao thức (MAVLink):
*   **MAVLink Parse logic:** Sử dụng `MemoryStream` kết hợp với `MAVLink.MavlinkParse` để duyệt qua từng byte thô nhận được từ UDP buffer nhằm ghép và khôi phục lại gói tin MAVLink hoàn chỉnh có cấu trúc chính xác (kiểm tra CRC).
*   **Cách sử dụng thư viện MAVLink C#:**
    *   Thư viện MAVLink C# là một lớp (Class) khổng lồ chứa các cấu trúc và định nghĩa tĩnh. Không thể dùng `using MAVLink;` mà phải gọi trực tiếp dạng `MAVLink.MAVLINK_MSG_ID` hoặc `MAVLink.mavlink_heartbeat_t`.
    *   Sử dụng phương thức `packet.ToStructure<T>()` để chuyển đổi tiêu đề byte thô của gói tin sang cấu trúc dữ liệu cụ thể tương ứng với từng ID gói tin.

### Về quy trình làm việc với AI:
*   **Phân chia lớp chức năng (Layered Architecture):** Tách biệt rõ ràng giữa lớp kết nối mạng (`UdpConnection`), lớp xử lý giao thức (`MavlinkService`) và lớp hiển thị giao diện (`Form1`). Giúp code cực kỳ sáng sủa, dễ bảo trì và dễ mở rộng.
*   **Sử dụng kinh nghiệm thực chiến:** Việc lưu giữ các bài học và code mẫu thành công (như đoạn Python test kết nối và hướng dẫn cấu hình `ReuseAddress`) giúp AI khoanh vùng và xử lý lỗi cực kỳ nhanh chóng.

---

## 4. Danh sách các Lệnh MAVLink / Cấu trúc dữ liệu đã sử dụng

| ID gói tin | Tên cấu trúc MAVLink | Chức năng & Dữ liệu khai thác |
| :--- | :--- | :--- |
| **`HEARTBEAT` (#0)** | `MAVLink.mavlink_heartbeat_t` | Xác nhận drone còn sống, đọc Chế độ bay (`custom_mode` cast sang `COPTER_MODE`) và trạng thái Arm/Disarm (`base_mode & MAV_MODE_FLAG.SAFETY_ARMED`). |
| **`SYS_STATUS` (#1)** | `MAVLink.mavlink_sys_status_t` | Đọc phần trăm pin còn lại (`battery_remaining`) và điện áp pin (`voltage_battery` đổi sang Volt bằng cách chia cho 1000.0). |
| **`ATTITUDE` (#30)** | `MAVLink.mavlink_attitude_t` | Lấy dữ liệu góc quay Pitch, Roll, Yaw (đơn vị Radian, cần nhân với `180 / Math.PI` để đổi sang Độ). |
| **`GLOBAL_POSITION_INT` (#33)** | `MAVLink.mavlink_global_position_int_t` | Đọc Kinh độ (`lon / 1E7`), Vĩ độ (`lat / 1E7`), Độ cao tương đối so với Home (`relative_alt / 1000.0` mét) và Vận tốc tức thời từ các thành phần vector `vx`, `vy`, `vz`. |

---

## 5. Tự đánh giá & Kế hoạch tiếp theo

*   **Mức độ hiểu bài:** [x] Rất tự tin
*   **Những rủi ro tiềm ẩn (Technical Debt) để lại cho Task sau:**
    *   Cổng kết nối UDP `14551` hiện tại đang được gán cứng (hardcode) trong code khởi chạy. Task sau cần xây dựng giao diện cấu hình kết nối để người dùng có thể tùy biến Cổng UDP, địa chỉ IP hoặc chuyển đổi sang kết nối Serial (COM Port + Baudrate).
    *   Hiện tại ứng dụng chỉ có một cửa sổ Log dạng text thô. Cần thiết kế bảng hiển thị thông số trực quan (Dashboard) và hệ thống nút bấm điều khiển gửi lệnh xuống SITL ở các giai đoạn sau.
