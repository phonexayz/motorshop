# 🚀 ขั้นตอนการ Deploy และอัปโหลดระบบขึ้น Render.com (ฉบับมือโปร)

หากในอนาคตคุณต้องการติดตั้งระบบนี้ใหม่ หรือสร้างโปรเจกต์ใหม่ ให้ทำตาม 5 ขั้นตอนนี้ครับ:

---

## ขั้นตอนที่ 1: เตรียมโค้ดของคุณ (ใน VS Code)
ตรวจสอบว่าในโฟลเดอร์โปรเจกต์ของคุณมีไฟล์เหล่านี้แล้ว:
1.  **`Dockerfile`**: หัวใจหลักที่บอก Render ว่าจะรันโปรแกรมยังไง
2.  **`Program.cs`**: ต้องมีส่วนที่อ่าน `DATABASE_URL` (แบบที่เราแก้กัน)
3.  **GitHub**: อัปโหลดโค้ดทั้งหมดขึ้น GitHub ของคุณ (`git push`)

---

## ขั้นตอนที่ 2: สร้างฐานข้อมูล (Render PostgreSQL)
1.  เข้าเว็บ [dashboard.render.com](https://dashboard.render.com)
2.  กด **New +** -> **PostgreSQL**
3.  ตั้งชื่อ และเลือก Region เป็น **Singapore** (ใกล้บ้านเราที่สุด)
4.  กดสร้าง (**Create**) และรอจนเสร็จ
5.  **สำคัญ**: ก๊อปปี้ค่า **Internal Database URL** (สำหรับในเว็บ) และ **External Database URL** (สำหรับการอัปโหลดข้อมูลจากเครื่อง) เก็บไว้

---

## ขั้นตอนที่ 3: สร้างหน้าเว็บ (Render Web Service)
1.  กด **New +** -> **Web Service**
2.  เลือก Repository `motorshop` จาก GitHub ของคุณ
3.  **Runtime**: เลือกเป็น **Docker**
4.  **Region**: เลือก **Singapore** ให้ตรงกับฐานข้อมูล
5.  ไปที่แถบ **Advanced** -> **Add Environment Variable**:
    *   `DATABASE_URL` = (วางค่า Internal Database URL ที่ก๊อปมา)
    *   `ASPNETCORE_ENVIRONMENT` = `Production`
6.  กด **Create Web Service** และรอจนขึ้นสีเขียวว่า **Live**

---

## ขั้นตอนที่ 4: อัปโหลดข้อมูลจริง (Database Migration)
เปิด Terminal ใน VS Code แล้วใช้คำสั่งนี้ (เปลี่ยนรหัสผ่านและที่อยู่ตามที่คุณได้จาก Render):

```powershell
$env:PGPASSWORD="รหัสผ่านของคุณ"; & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h ที่อยู่โฮสต์.render.com -U ชื่อผู้ใช้ -d ชื่อฐานข้อมูล -f motorshop_final.sql
```

---

## ขั้นตอนที่ 5: ตรวจสอบและใช้งาน
1.  เปิดลิงก์ `.onrender.com` ของคุณ
2.  ล็อกอินด้วย Username และ Password ของคุณ
3.  หากเข้าไม่ได้ ให้เช็กไฟล์ `Program.cs` ว่ามีส่วนของ **Rescue Admin** หรือไม่

---

> [!TIP]
> **เคล็ดลับ**: ทุกครั้งที่แก้ไขโค้ด เพียงแค่ `git push` เข้า GitHub... Render จะอัปเดตเว็บให้คุณโดยอัตโนมัติภายใน 3 นาทีครับ!
