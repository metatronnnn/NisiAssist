# เผยแพร่ Nisi Assist ให้เพื่อนใช้ (Custom Plugin Repository)

เพื่อนติดตั้งและอัปเดตผ่าน `/xlplugins` ได้เอง ไม่ต้อง build เอง

## ฝั่งคุณ (ทำครั้งเดียว)

1. สมัคร/ล็อกอิน https://github.com
2. สร้าง repository ใหม่ ชื่อ `NisiAssist` ตั้งเป็น **Public** (ต้องเป็น Public ลิงก์ถึงจะเข้าถึงได้)
3. อัปโหลดไฟล์ทั้งหมดในโฟลเดอร์นี้ขึ้นไป (ลากวางในหน้าเว็บ "Add file > Upload files" ได้)
   - ต้องมีโฟลเดอร์ `.github/workflows/build.yml` ด้วย (โฟลเดอร์ที่ขึ้นต้นด้วยจุดอาจถูกซ่อนในเครื่อง ให้เปิดแสดงไฟล์ซ่อน)
   - อย่าอัปโหลดโฟลเดอร์ `bin` และ `obj`
4. แท็บ **Settings > Actions > General > Workflow permissions** เลือก **Read and write permissions** แล้ว Save
5. แท็บ **Actions** รอให้ workflow "Build and publish" ขึ้นเครื่องหมายถูกสีเขียว (ถ้าไม่รันเอง กด Run workflow)
6. จะได้ไฟล์ `repo.json` ใน repo และ release ชื่อ `latest`

## ลิงก์ที่ให้เพื่อน

```
https://raw.githubusercontent.com/<ชื่อ GitHub ของคุณ>/NisiAssist/main/repo.json
```

## ฝั่งเพื่อน

1. พิมพ์ `/xlsettings` > แท็บ **Experimental**
2. ช่อง **Custom Plugin Repositories** วางลิงก์ด้านบน > กดเครื่องหมาย + > **Save and Close**
3. พิมพ์ `/xlplugins` > **All Plugins** ค้นหา "Nisi Assist" > Install
4. พิมพ์ `/nisi preview` เพื่อวางตำแหน่ง (อ่านวิธีใช้ใน USER_GUIDE.md)

## อัปเดตเวอร์ชัน

แก้โค้ด เพิ่มเลข `<Version>` ใน `NisiAssist.csproj` แล้วอัปโหลดทับ workflow จะ build และอัปเดต `repo.json` ให้เอง
เพื่อนจะเห็นปุ่ม Update ใน `/xlplugins`

## ข้อควรรู้

- ปลั๊กอินของ Dalamud ไม่ได้รับอนุญาตจาก Square Enix เพื่อนใช้โดยรับความเสี่ยงเอง (ควรบอกเพื่อนก่อน)
- repo สาธารณะ ทุกคนเห็นโค้ด อย่าใส่ข้อมูลส่วนตัวในไฟล์
- ถ้าอยากให้เฉพาะเพื่อน ส่งไฟล์ `NisiAssist.dll` + โฟลเดอร์ `images`/`icons` ให้ไปเพิ่มใน Dev Plugin Locations เองก็ได้ แต่อัปเดตยากกว่า
