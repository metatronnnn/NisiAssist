# Nisi Assist (Dalamud plugin) — TEA เฟส 2, Nisi pass รอบ 3 และ 4

โอเวอร์เลย์ที่บอกว่า "รอบนี้ต้องส่ง/รับ Nisi ตัวไหน กับใคร" ตอน Brute Justice / Cruise Chaser

## ทำอะไรได้
- **รอบ 3** (หลัง Flarethrower) และ **รอบ 4** (ก่อน Propeller Wind / หลังก้อนน้ำแข็ง)
- ถ้าคุณถือ Nisi อยู่ → `PASS ALPHA -> ชื่อคนที่ต้องรับ`
- ถ้าคุณไม่ได้ถือ → `GET ALPHA <- ชื่อคนที่ถืออยู่`
- นับถอยหลังช่วงเวลาแลก + เวลาที่เหลือของ Nisi ที่คุณถือ
- โหมด "Only me" (ค่าเริ่มต้น) แสดงเฉพาะของตัวเอง ปิดได้ใน settings เพื่อดูตารางทั้งปาร์ตี้

## กติกา (อ้างอิงจาก Cactbot `the_epic_of_alexander.ts`)
- Nisi ที่ถือ α/β/γ/δ = status `0x8AE, 0x8AF, 0x859, 0x85A`
- Verdict (ตัวที่ต้องรับ) α/β/γ/δ = status `0x8B0, 0x8B1, 0x85B, 0x85C`
- ถือ Nisi → ส่งให้คนที่ verdict ตรงกัน (เลือกคนที่ยังไม่ถือ Nisi ก่อน)
- ไม่ถือ Nisi → ไปรับ Nisi ที่ตรงกับ verdict ของตัวเอง
- รอบ 3 = 8 วิ หลัง Brute Justice เริ่มร่าย Flarethrower (`0x4845`) นาน 9 วิ
- รอบ 4 = 15 วิ ก่อน Propeller Wind (`0x4832`) นาน 14 วิ

## ติดตั้ง (แบบ Custom Repository ไม่ต้อง build เอง)

1. ใช้ XIVLauncher ที่เปิด Dalamud
2. ในเกมพิมพ์ `/xlsettings` → แท็บ **Experimental**
3. ช่อง **Custom Plugin Repositories** วางลิงก์นี้ แล้วกดเครื่องหมาย **+** → **Save and Close**
   ```
   https://raw.githubusercontent.com/metatronnnn/NisiAssist/main/repo.json
   ```
4. พิมพ์ `/xlplugins` → **All Plugins** → ค้นหา `nisi` → **Install**
5. เมื่อมีเวอร์ชันใหม่ จะมีปุ่ม **Update** ใน `/xlplugins` กดอัปเดตได้เลย

> ปลั๊กอินของ Dalamud ไม่ได้รับอนุญาตจาก Square Enix อย่างเป็นทางการ ใช้โดยเข้าใจความเสี่ยงเอง
> ถ้าเคยติดตั้งแบบ Dev Plugin (build เอง) ให้ปิดและลบอันนั้นก่อน ไม่งั้นจะซ้ำกัน

## สำหรับผู้พัฒนา (build เอง)
`dotnet build -c Release` แล้วเพิ่มพาธ `NisiAssist.dll` ใน `/xlsettings` → Experimental → Dev Plugin Locations
(ดู PUBLISH_GUIDE.md สำหรับการเผยแพร่)

## คำสั่ง
- `/nisi` เปิดหน้า settings
- `/nisi preview` เปิด/ปิดตัวอย่าง (ใช้ลากวางตำแหน่ง)
- `/nisi lock` ล็อก/ปลดล็อกตำแหน่ง

## ข้อจำกัดที่ควรรู้
- ตัวหนังสือในโอเวอร์เลย์เป็นภาษาอังกฤษ เพราะฟอนต์ ImGui ของ Dalamud ไม่มีอักษรไทย
- เวลาเป็นค่ากลางจาก Cactbot timeline อาจคลาดเคลื่อน ±1 วินาที ปรับได้ใน settings
- ยังไม่ได้ทดสอบในเกมจริง และยังไม่ได้ compile (ดู `ApplyFontScale` ใน `OverlayWindow.cs` ถ้า build แล้วฟ้อง error เรื่อง font scale)
