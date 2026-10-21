# Kenney asset location

The setup tool first tries to find the local download folder:

C:\Users\hodac\Downloads\kenney_space-shooter-remastered

If found, it automatically copies Backgrounds/, PNG/, Bonus/ and license.txt into this directory. If the project is running on another Windows account or the asset folder is elsewhere, manually copy those folders here while preserving their internal structure.

Expected example: Assets/Art/KenneySpaceShooter/PNG/playerShip1_blue.png.

Then run Tools → Space Explorer → Tạo toàn bộ dự án in Unity. The editor tool imports PNG files as Sprites and rebuilds the scenes/prefabs using these assets. If Kenney assets are not available, it creates fallback sprites so the game can still be tested.
