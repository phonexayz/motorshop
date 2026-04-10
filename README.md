# ร้านซ่อมมอเตอร์ไซค์ Management System

ระบบจัดการร้านซ่อมมอเตอร์ไซค์สมบูรณ์แบบ สร้างด้วย ASP.NET Core MVC 9.0 และ PostgreSQL

## 🚀 Features

### 📦 จัดการสต็อกอะไหล่
- ✅ CRUD สต็อกอะไหล่ (เพิ่ม/ลบ/แก้ไข/ดูรายละเอียด)
- ✅ ค้นหาอะไหล่ตามชื่อหรือรุ่นรถที่รองรับ
- ✅ แจ้งเตือนสต็อกต่ำอัตโนมัติ
- ✅ สแกนบาร์โค้ดด้วยกล้องหรือป้อนด้วยมือ
- ✅ อัพเดทสต็อกแบบ Real-time

### 🔧 จัดการใบสั่งซ่อม
- ✅ สร้างใบสั่งซ่อมพร้อมเลขที่อัตโนมัติ
- ✅ จัดการสถานะ (Pending, InProgress, Completed)
- ✅ เพิ่มอะไหล่และบริการในใบสั่งซ่อม
- ✅ คำนวณราคาอัตโนมัติ (ค่าอะไหล่ + ค่าแรง)
- ✅ แดชบอร์ดสำหรับช่างซ่อม

### 👥 ผู้ใช้งาน
- ✅ ระบบ Login/Logout
- ✅ จัดการสิทธิ์ผู้ใช้ (Admin, Staff)
- ✅ Session Management
- ✅ Password Hashing ความปลอดภัยสูง

### 📊 แดชบอร์ด
- ✅ ภาพรวมสถิติร้าน
- ✅ แจ้งเตือนสต็อกต่ำ
- ✅ รายการใบสั่งซ่อมล่าสุด
- ✅ สถิติงานวันนี้

## 🛠️ Technology Stack

- **Frontend**: ASP.NET Core MVC 9.0
- **Backend**: .NET 9.0 Framework
- **Database**: PostgreSQL (รองรับ Supabase และ Azure SQL)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: Session-based with SHA256 Hashing
- **UI**: Bootstrap 5 + Bootstrap Icons
- **Barcode**: ZXing.Net Library

## 📋 การติดตั้ง

### 1. Prerequisites
- .NET 9.0 SDK
- PostgreSQL หรือ Supabase Account
- Visual Studio 2022 หรือ VS Code

### 2. Clone & Setup
```bash
git clone <repository-url>
cd motorshop
```

### 3. Database Setup

#### Option A: Supabase (แนะนำ)
1. สร้างโปรเจกต์ใหม่ที่ [Supabase](https://supabase.com)
2. รับ Connection String จาก Settings > Database
3. อัพเดท `appsettings.json`

#### Option B: Local PostgreSQL
```bash
# สร้าง database
createdb motorshop

# รัน schema
psql -d motorshop -f Database/Schema.sql
```

### 4. Configuration
อัพเดท `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Port=5432;Username=postgres;Password=your-password;Database=motorshop"
  }
}
```

### 5. Run Migrations
```bash
dotnet ef database update
```

### 6. Run Application
```bash
dotnet run
```

เข้าใช้งานที่: `https://localhost:5001`

## 👤 ผู้ใช้ทดสอบ

| Username | Password | Role |
|----------|----------|------|
| admin | password | Admin |
| mechanic1 | password | Staff |

## 📱 การใช้งาน

### หน้าหลัก (Dashboard)
- ดูภาพรวมสถิติร้าน
- แจ้งเตือนสต็อกต่ำ
- รายการใบสั่งซ่อมล่าสุด

### จัดการสต็อก (Inventory)
- เพิ่ม/แก้ไขอะไหล่ใหม่
- ค้นหาและกรองข้อมูล
- สแกนบาร์โค้ดเพื่อค้นหาอะไหล่
- อัพเดทจำนวนสต็อก

### ใบสั่งซ่อม (Repair Orders)
- สร้างใบสั่งซ่อมใหม่
- เพิ่มอะไหล่และบริการ
- อัพเดทสถานะการซ่อม
- คำนวณราคาอัตโนมัติ

### แดชบอร์ดช่างซ่อม
- ดูงานที่รอดำเนินการ
- เริ่ม/เสร็จสิ้นการซ่อม
- บันทึกค่าแรงและหมายเหตุ

## 🔧 Database Schema

### ตารางหลัก:
- **Users** - ข้อมูลผู้ใช้
- **Customers** - ข้อมูลลูกค้าและรถมอเตอร์ไซค์
- **Parts** - ข้อมูลอะไหล่ (พร้อมบาร์โค้ดและรุ่นที่รองรับ)
- **Services** - บริการซ่อมและค่าแรง
- **RepairOrders** - ใบสั่งซ่อม
- **OrderDetails** - รายละเอียดอะไหล่/บริการในแต่ละใบสั่งซ่อม

## 📝 Notes

- ระบบใช้ Session-based Authentication สำหรับความเรียบง่าย
- รองรับการสแกนบาร์โค้ดผ่านกล้องมือถือ
- คำนวณราคาอัตโนมัติเมื่อเพิ่มอะไหล่/บริการ
- แจ้งเตือนเมื่อสต็อกต่ำกว่าที่กำหนด
- รองรับการทำงานบน Cloud Database (Supabase/Azure)

## 🚀 การพัฒนาต่อ

สามารถเพิ่มฟีเจอร์เพิ่มเติมได้:
- รายงานสรุปยอดขาย
- ระบบนัดหมายลูกค้า
- การพิมพ์ใบเสร็จ/ใบสั่งซ่อม
- Mobile App Version
- Integration กับระบบบัญชี
- SMS/Email Notification

## 📄 License

MIT License
