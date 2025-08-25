
## 🚀 Demo Özellikleri

### 🔹 Core Mekanikler
- **Grid Sistemi**: ScriptableObject tabanlı dinamik grid oluşturma (3D düzen, orthographic kamera).
- **GoalBox Sistemi**: Renk bazlı hedefler; minimum–maksimum dağılım kuralları ile hedef sayıları.
- **Launcher Sistemi**:  
  - 5 slotlu launcher box sistemi  
  - GoalItem tıklama ve otomatik yerleşim  
  - Merge mekaniği (aynı renkten 3 item birleşip değerleri toplar)  
- **Bullet Manager**:  
  - Mermi hareketi (frame bağımsız, Coroutine tabanlı)  
  - Yol üzerindeki tile’lara squash + push animasyonu  
  - Object Pooling ile performanslı mermi yönetimi

### 🔹 Game Feel & Polish
- DOTween animasyonları (scale, bounce, fade, jump)
- Tile squash & push juice efektleri
- Merge sonrası particle + kamera shake
- Haptic feedback (tıklama, merge, fire)
- Ses efektleri: merge, squash, box destroy
- Smooth UI animasyonları (goal box aç/kapa, counter düşüşleri)

---

## 🛠️ Teknik Mimari

### Kullanılan Araçlar & Yapılar
- **Unity (C#)**  
- **Zenject**: Dependency Injection (GridManager, TilePool, BulletPool, LauncherManager gibi bileşenler)
- **DOTween**: Animasyonlar (scale, squash, DOJump, fade-out)
- **ScriptableObject**: GridData, Tile renk-ID eşleştirmeleri
- **Object Pooling**: Tile, Bullet, Particle gibi objelerde performanslı kullanım
- **Event Driven Architecture**: UnityAction & Custom Events ile loose coupling
- **OOP & SOLID Prensipleri**:  
  - **SRP**: GridManager yalnızca grid yönetir, BulletManager yalnızca mermilerden sorumludur  
  - **OCP**: Yeni power-up, booster veya blocker sistemleri kolayca eklenebilir  
  - **DIP**: Zenject üzerinden bağımlılıklar enjekte edilmiştir 

https://github.com/user-attachments/assets/c8719753-6370-4cea-a821-663fa5a21503

