# 🚀 Space Explorer — Game 2D Unity (giao diện tiếng Việt)

Dự án game bắn phi thuyền 2D phục vụ bài thực hành Unity: GameObject/Transform, Component, scene, prefab, physics 2D, collider, UI, điểm số và chuyển cảnh. Dùng bộ hình **Kenney Space Shooter Remastered** khi bạn chép asset vào dự án.

> **Lần đầu cần chạy công cụ dựng dự án một lần:** menu Unity **Tools → Space Explorer → Tạo toàn bộ dự án**. Công cụ sẽ tạo ba scene, bốn prefab, UI và Build Settings tự động; không cần tự kéo từng GameObject.

## 1. Tính năng

- **Menu chính:** nút Bắt đầu, Hướng dẫn, Thoát; bảng hướng dẫn tiếng Việt.
- **Gameplay:** điều khiển tàu bằng phím mũi tên hoặc WASD; giữ Space hoặc nhấn chuột trái để bắn.
- **Thiên thạch:** sinh ở vị trí ngẫu nhiên, trôi và xoay ngẫu nhiên, quay lại ở mép màn hình; bổ sung thiên thạch theo thời gian.
- **Sao thưởng:** xuất hiện rải rác, có hiệu ứng lơ lửng/xoay; thu thập sao để cộng điểm.
- **Va chạm:** chạm thiên thạch bị trừ điểm và kết thúc lượt chơi; bắn trúng thiên thạch được điểm thưởng.
- **HUD:** cập nhật điểm ngay khi nhặt sao hoặc bắn trúng thiên thạch.
- **Màn kết thúc:** hiển thị điểm lượt chơi, kỷ lục đã lưu, chơi lại, về menu hoặc thoát.
- **Âm thanh:** dùng âm thanh Kenney nếu có asset; game vẫn có thể chạy với hình dự phòng nếu chưa chép bộ asset.
- **Scene và Build Settings:** tự tạo MainMenu, Gameplay, EndGame và thêm vào danh sách scene build.

## 2. Yêu cầu

- Unity **6000.6.6f1** (hoặc bản Unity 6 tương thích).
- Git (nếu tải qua clone).
- Bộ asset kenney_space-shooter-remastered tải sẵn trên máy.

## 3. Lấy mã nguồn

    git clone https://github.com/khanh0102102/space-explorer-unity.git

Mở Unity Hub → **Add project from disk** → chọn thư mục space-explorer-unity. Chờ Unity hoàn tất import Package Manager và biên dịch C#.

## 4. Thêm asset Kenney vào đúng cấu trúc

Trong Unity Project, tạo hoặc mở thư mục:

**Assets/Art/KenneySpaceShooter/**

Từ thư mục tải về **C:\Users\hodac\Downloads\kenney_space-shooter-remastered\\**, **chép các thư mục Backgrounds, PNG, Bonus và file license.txt vào bên trong Assets/Art/KenneySpaceShooter/**. Giữ nguyên cây thư mục, ví dụ:

    Assets/
    └── Art/
        └── KenneySpaceShooter/
            ├── license.txt
            ├── Backgrounds/
            │   └── darkPurple.png
            ├── Bonus/
            │   ├── sfx_laser1.ogg
            │   ├── sfx_lose.ogg
            │   └── ...
            └── PNG/
                ├── playerShip1_blue.png
                ├── Lasers/
                ├── Meteors/
                ├── Power-ups/
                └── ...

Không cần chép preview.png, sample.png, Spritesheet hoặc Vector để game chạy. Giữ lại license.txt để ghi nhận giấy phép của bộ asset.

## 5. Tạo scene và prefab tự động

1. Trong Unity, chờ không còn tiến trình import/compile.
2. Trên thanh menu chọn **Tools → Space Explorer → Tạo toàn bộ dự án**.
3. Khi hiện thông báo hoàn tất, mở **Assets/Scenes/MainMenu.unity**.
4. Nhấn **Play** để thử từ menu chính.

Công cụ import PNG trong thư mục Kenney thành Sprite, lấy hình tàu/thiên thạch/sao/laser/nền và lấy âm thanh nếu có. Nếu chưa chép asset, công cụ tự tạo hình dự phòng để bạn vẫn kiểm tra được luồng chơi. Sau đó có thể chép asset rồi chạy lại công cụ để dựng lại scene/prefab bằng hình Kenney.

Các mục **Tools → Space Explorer → Mở scene Menu** và **Mở scene Gameplay** dùng để mở nhanh từng scene sau khi đã tạo.

## 6. Điều khiển và luật tính điểm

| Thao tác | Kết quả |
|---|---|
| Phím mũi tên hoặc W A S D | Di chuyển phi thuyền theo bốn hướng |
| Giữ Space | Bắn laser |
| Nhấn giữ chuột trái | Bắn laser (tùy chọn bổ sung) |
| Thu thập một ngôi sao | +10 điểm |
| Laser bắn trúng thiên thạch | +5 điểm |
| Phi thuyền đâm vào thiên thạch | -10 điểm, sau đó chuyển sang End Game |

Điểm được hiển thị ngay trên HUD; điểm cuối và kỷ lục được lưu bằng PlayerPrefs trên máy chạy game.

## 7. Cấu trúc dự án

    Assets/
    ├── Editor/
    │   └── SpaceExplorerProjectSetup.cs  # Công cụ tạo scene/prefab/UI
    ├── Scripts/
    │   ├── Core/GameManager.cs           # Điểm số, kết thúc và chuyển scene
    │   ├── Player/PlayerController.cs    # Điều khiển tàu và bắn laser
    │   ├── Gameplay/
    │   │   ├── AsteroidMotion.cs         # Di chuyển, quay, va chạm thiên thạch
    │   │   ├── LaserProjectile.cs        # Đạn và xử lý bắn trúng
    │   │   ├── SpawnManager.cs           # Sinh thiên thạch và sao ngẫu nhiên
    │   │   └── StarCollectible.cs        # Thu thập sao
    │   └── UI/
    │       ├── HUDController.cs          # Điểm trên màn chơi
    │       ├── MainMenuUI.cs             # Menu và hướng dẫn
    │       └── EndGameUI.cs              # Điểm cuối và nút điều hướng
    ├── Art/
    │   ├── KenneySpaceShooter/           # Chép asset Kenney vào đây
    │   └── Generated/                    # Hình dự phòng tự tạo
    ├── Prefabs/                          # Công cụ tạo tự động
    └── Scenes/                           # Công cụ tạo tự động

## 8. Khớp với rubric

- **Functionality (50):** di chuyển/bắn, thiên thạch di chuyển ngẫu nhiên, sao cộng điểm, va chạm trừ điểm, game over và chuyển scene.
- **Creativity (20):** phong cách không gian tím/xanh, HUD rõ ràng, hiệu ứng sao, âm thanh Kenney, kỷ lục, điểm thưởng khi bắn thiên thạch.
- **Documentation (30):** mã có XML summary/ghi chú tiếng Việt; xem **Docs/BAO_CAO_DU_AN.md** để hiểu kiến trúc, luồng scene và checklist kiểm thử.

## 9. Kiểm thử nhanh

- [ ] Mở game thấy Menu tiếng Việt.
- [ ] Nút Hướng dẫn mở/đóng panel đúng.
- [ ] Bắt đầu chuyển sang Gameplay.
- [ ] Tàu di chuyển được bằng phím mũi tên/WASD và không bay khỏi camera.
- [ ] Giữ Space bắn được laser.
- [ ] Sao biến mất và điểm tăng 10 ngay khi thu thập.
- [ ] Bắn trúng thiên thạch, thiên thạch biến mất và điểm tăng 5.
- [ ] Đâm vào thiên thạch, điểm giảm 10 và chuyển sang End Game.
- [ ] Điểm cuối/kỷ lục hiển thị; Chơi lại và Về menu hoạt động.
- [ ] MainMenu, Gameplay, EndGame đều xuất hiện trong Build Settings.

## Ghi chú

Scene và prefab được tạo từ công cụ Editor để tránh yêu cầu bạn dựng thủ công trong Unity. Sau lần tạo đầu tiên, nên commit các file .unity, .prefab và .meta được Unity sinh ra nếu muốn lưu scene đã tạo lên GitHub.
