# 🏍️ คู่มือการดูแลระบบ Motorcycle Shop System (Render.com Edition)

คู่มือนี้สรุปขั้นตอนสำคัญทั้งหมดที่คุณทำสำเร็จในวันนี้ เพื่อใช้เป็นแหล่งอ้างอิงในอนาคต

---

## 1. ลิงก์สำคัญของคุณ
*   **หน้าเว็บไซต์**: [https://motorshop-app-zu0k.onrender.com](https://motorshop-app-zu0k.onrender.com)
*   **ระบบจัดการ Render**: [https://dashboard.render.com](https://dashboard.render.com)

---

## 2. วิธีการอัปเดตระบบ (GitHub)
เมื่อคุณมีการแก้ไขโค้ดในโปรแกรม VS Code และต้องการให้บนเว็บเปลี่ยนตาม ให้รัน 3 คำสั่งนี้ใน Terminal:
1.  `git add .` (เตรียมไฟล์)
2.  `git commit -m "อธิบายสิ่งที่แก้ไข"` (บันทึกการเปลี่ยนแปลง)
3.  `git push` (ส่งขึ้น GitHub และ Render จะอัปเดตอัตโนมัติ)

---

## 4. การจัดการฐานข้อมูล (Database)
ข้อมูลของคุณถูกเก็บไว้ที่ **Render PostgreSQL** หากต้องการสำเนาหรือกู้คืนข้อมูล ให้ใช้คำสั่งนี้:

### การส่งข้อมูลจากเครื่องขึ้นไปบน Cloud:
```powershell
$env:PGPASSWORD="O2F8hCG0GrPnoJq1Nf9Np2qOsePSjURY"; & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h dpg-d7ca38l7vvec73b39q80-a.singapore-postgres.render.com -U motorshop_db_m17i_user -d motorshop_db_m17i -f motorshop_final.sql
```

---

## 5. ระบบกู้ภัย (Rescue Admin)
หากวันใดวันหนึ่งคุณ "ลืมรหัสผ่าน" หรือเข้าบ้านไม่ได้:
*   ผมได้ฝังโค้ดพิเศษไว้ให้แล้ว คือคุณสามารถใช้ชื่อผู้ใช้ `admin` และรหัสผ่าน `password` เข้าไปได้เสมอ
*   **คำแนะนำ**: เมื่อเข้าได้แล้ว ควรเข้าไปเปลี่ยนรหัสผ่านของคนอื่นในหน้าจัดการผู้ใช้ทันที

---

## 6. การตั้งค่าตัวแปร (Environment Variables)
ในหน้า Web Service ของ Render (แถบ Variables) ต้องมี 2 ค่านี้เสมอ:
*   `DATABASE_URL`: ลิงก์ Internal จากหน้า Database
*   `ASPNETCORE_ENVIRONMENT`: `Production`

---

> [!NOTE]
> **ความปลอดภัย**: รหัสผ่านในไฟล์ SQL และในคำสั่งด้านบนเป็นข้อมูลสำคัญ ควรระมัดระวังในการแชร์ไฟล์นี้กับผู้อื่นครับ
