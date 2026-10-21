# Tài liệu dự án — Space Explorer

## 1. Mục tiêu

Space Explorer là trò chơi phi thuyền 2D thực hành những khái niệm Unity cơ bản: Scene, GameObject, Transform, Component, Prefab, SpriteRenderer, Rigidbody2D, Collider2D, UI và SceneManager. Tất cả nội dung UI dành cho người chơi hiển thị bằng tiếng Việt.

## 2. Kiến trúc

| Thành phần | Trách nhiệm |
|---|---|
| GameManager | Giữ điểm lượt chơi, cập nhật HUD, xử lý va chạm kết thúc, lưu điểm cuối/kỷ lục và điều hướng scene |
| PlayerController | Đọc phím mũi tên/WASD, giới hạn tàu trong màn hình và bắn laser theo thời gian hồi |
| SpawnManager | Sinh các prefab thiên thạch/sao tại tọa độ ngẫu nhiên trong camera |
| AsteroidMotion | Chọn hướng/tốc độ/góc xoay ngẫu nhiên, di chuyển và quấn về mép bên kia màn hình |
| LaserProjectile | Di chuyển hướng lên, tự hủy sau thời gian giới hạn, cộng điểm khi chạm thiên thạch |
| StarCollectible | Làm sao lơ lửng/xoay, cộng điểm khi người chơi thu thập |
| HUDController | Hiển thị điểm hiện tại ngay khi điểm thay đổi |
| MainMenuUI | Bắt đầu game, hiện/ẩn hướng dẫn, thoát |
| EndGameUI | Hiển thị điểm cuối/kỷ lục, chơi lại, về menu, thoát |
| SpaceExplorerProjectSetup | Tạo prefab, ba scene, canvas/UI và cấu hình Build Settings trong Unity Editor |

Các script gameplay dùng component và collider thay vì kiểm tra tọa độ va chạm thủ công. Các prefab được sinh từ sprite đã nhập, nên có thể thay asset mà không cần thay logic.

## 3. Scene và luồng trò chơi

1. MainMenu: tên game, nút Bắt đầu, Hướng dẫn và Thoát.
2. Gameplay: nền không gian, tàu người chơi, HUD điểm, SpawnManager, thiên thạch và sao.
3. EndGame: điểm cuối, kỷ lục, Chơi lại, Về menu chính và Thoát.

Luồng bình thường là MainMenu → Gameplay → EndGame. Chọn Chơi lại sẽ tải Gameplay; chọn Về menu chính sẽ tải MainMenu.

## 4. Luật chơi

- Di chuyển: phím mũi tên hoặc WASD.
- Bắn: giữ Space hoặc nhấn giữ chuột trái.
- Nhặt sao: +10 điểm.
- Bắn trúng thiên thạch: +5 điểm và phá hủy thiên thạch.
- Đâm vào thiên thạch: -10 điểm và kết thúc lượt chơi.
- Điểm cuối và kỷ lục được lưu cục bộ bằng PlayerPrefs.

## 5. Asset

Dự án dự kiến sử dụng Kenney Space Shooter Remastered:
- Nền: Backgrounds/darkPurple.png.
- Tàu: PNG/playerShip1_blue.png.
- Thiên thạch: PNG/Meteors/meteorBrown_big1.png.
- Sao: PNG/Power-ups/star_gold.png.
- Laser: PNG/Lasers/laserBlue01.png.
- Âm thanh: Bonus/sfx_laser1.ogg, Bonus/sfx_lose.ogg, Bonus/sfx_twoTone.ogg.

Sao chép thư mục Backgrounds, PNG, Bonus cùng license.txt vào Assets/Art/KenneySpaceShooter/. Công cụ Editor tự đổi cấu hình PNG thành Sprite. Nếu ảnh không có, công cụ tạo sprite dự phòng trong Assets/Art/Generated.

## 6. Collider và Physics 2D

- Tàu: Rigidbody2D không chịu trọng lực, CircleCollider2D dạng trigger.
- Thiên thạch: CircleCollider2D dạng trigger; va chạm được xử lý trong AsteroidMotion.
- Sao: CircleCollider2D dạng trigger; xử lý trong StarCollectible.
- Laser: BoxCollider2D dạng trigger và Rigidbody2D kinematic; xử lý trong LaserProjectile.

Trigger phù hợp với trò chơi arcade để phát hiện va chạm mà không đẩy tàu/thiên thạch theo lực vật lý.

## 7. Kịch bản kiểm thử

| ID | Thao tác | Kết quả mong đợi |
|---|---|---|
| TC-01 | Chạy từ Menu và bấm Hướng dẫn | Panel hướng dẫn hiện ra |
| TC-02 | Bấm Đã hiểu | Panel đóng lại |
| TC-03 | Bấm Bắt đầu phiêu lưu | Mở Gameplay, điểm bắt đầu bằng 0 |
| TC-04 | Nhấn phím mũi tên/WASD | Tàu di chuyển trong giới hạn camera |
| TC-05 | Giữ Space | Laser được tạo và bay lên |
| TC-06 | Tàu chạm sao | Sao biến mất; điểm cộng 10 ngay lập tức |
| TC-07 | Laser chạm thiên thạch | Thiên thạch biến mất; điểm cộng 5 |
| TC-08 | Tàu chạm thiên thạch | Điểm trừ 10; chuyển End Game |
| TC-09 | Kiểm tra End Game | Điểm cuối và kỷ lục hiển thị |
| TC-10 | Bấm Chơi lại | Gameplay khởi động lại từ 0 điểm |
| TC-11 | Bấm Về menu chính | Mở MainMenu |

## 8. Ảnh chụp nên có trong báo cáo

1. Menu chính và panel Hướng dẫn.
2. Gameplay hiển thị tàu, sao, thiên thạch và HUD điểm.
3. Tàu bắn laser.
4. HUD sau khi thu thập sao (điểm tăng).
5. Màn End Game có điểm cuối/kỷ lục.
6. Unity Hierarchy/Project cho thấy ba scene và các prefab.
7. Build Settings hiển thị ba scene theo thứ tự.

## 9. Quy trình tạo dự án

1. Mở thư mục dự án bằng Unity Hub.
2. Chờ Package Manager và biên dịch hoàn tất.
3. Chép asset Kenney vào đúng thư mục như README.
4. Chọn Tools → Space Explorer → Tạo toàn bộ dự án.
5. Mở Assets/Scenes/MainMenu.unity, nhấn Play và chạy checklist kiểm thử.
6. Kiểm tra Build Settings; ba scene đã được thêm tự động.
