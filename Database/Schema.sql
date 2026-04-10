-- Motorcycle Repair Shop Database Schema (PostgreSQL)

-- Drop tables if they exist (for development)
DROP TABLE IF EXISTS OrderDetails CASCADE;
DROP TABLE IF EXISTS RepairOrders CASCADE;
DROP TABLE IF EXISTS Parts CASCADE;
DROP TABLE IF EXISTS Services CASCADE;
DROP TABLE IF EXISTS Customers CASCADE;
DROP TABLE IF EXISTS Users CASCADE;

-- Users table
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) DEFAULT 'Staff',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Customers table
CREATE TABLE Customers (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    Address TEXT,
    LicensePlate VARCHAR(20) NOT NULL,
    MotorcycleBrand VARCHAR(50) NOT NULL,
    MotorcycleModel VARCHAR(50) NOT NULL,
    Year INTEGER,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parts table (อะไหล่)
CREATE TABLE Parts (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    PartNumber VARCHAR(50) UNIQUE,
    Barcode VARCHAR(50),
    Description TEXT,
    CompatibleModels TEXT[], -- Array of compatible motorcycle models
    Price DECIMAL(10,2) NOT NULL,
    Cost DECIMAL(10,2),
    StockQuantity INTEGER DEFAULT 0,
    MinStockLevel INTEGER DEFAULT 5,
    Unit VARCHAR(20) DEFAULT 'ชิ้น',
    Supplier VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Services table (บริการซ่อม)
CREATE TABLE Services (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    BasePrice DECIMAL(10,2) NOT NULL,
    EstimatedHours DECIMAL(4,2),
    Category VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Repair Orders table (ใบสั่งซ่อม)
CREATE TABLE RepairOrders (
    Id SERIAL PRIMARY KEY,
    OrderNumber VARCHAR(20) UNIQUE NOT NULL,
    CustomerId INTEGER REFERENCES Customers(Id),
    MotorcycleLicensePlate VARCHAR(20) NOT NULL,
    ProblemDescription TEXT NOT NULL,
    Status VARCHAR(20) DEFAULT 'Pending', -- Pending, InProgress, Completed, Cancelled
    TotalAmount DECIMAL(10,2) DEFAULT 0,
    LaborCost DECIMAL(10,2) DEFAULT 0,
    PartsCost DECIMAL(10,2) DEFAULT 0,
    MechanicNotes TEXT,
    CompletionDate TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Order Details table (รายละเอียดใบสั่งซ่อม)
CREATE TABLE OrderDetails (
    Id SERIAL PRIMARY KEY,
    RepairOrderId INTEGER REFERENCES RepairOrders(Id) ON DELETE CASCADE,
    PartId INTEGER REFERENCES Parts(Id),
    ServiceId INTEGER REFERENCES Services(Id),
    Quantity INTEGER DEFAULT 1,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Discount DECIMAL(5,2) DEFAULT 0,
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX idx_customers_license_plate ON Customers(LicensePlate);
CREATE INDEX idx_parts_barcode ON Parts(Barcode);
CREATE INDEX idx_parts_stock ON Parts(StockQuantity);
CREATE INDEX idx_repair_orders_status ON RepairOrders(Status);
CREATE INDEX idx_repair_orders_date ON RepairOrders(CreatedAt);
CREATE INDEX idx_order_details_repair_order ON OrderDetails(RepairOrderId);

-- Insert sample data
INSERT INTO Users (Username, PasswordHash, Role) VALUES 
('admin', '$2a$10$YourHashedPasswordHere', 'Admin'),
('mechanic1', '$2a$10$YourHashedPasswordHere', 'Staff');

INSERT INTO Customers (Name, Phone, LicensePlate, MotorcycleBrand, MotorcycleModel, Year) VALUES 
('สมชาย ใจดี', '0812345678', 'กข-1234', 'Honda', 'Wave 110', 2020),
('สมศรี รักดี', '0823456789', 'กค-5678', 'Yamaha', 'Fino', 2021);

INSERT INTO Parts (Name, PartNumber, Barcode, CompatibleModels, Price, StockQuantity) VALUES 
('น้ำมันเครื่อง', 'EO-001', '8851234567890', ARRAY['Honda Wave 110', 'Yamaha Fino'], 150.00, 20),
('กรองอากาศ', 'AF-001', '8851234567891', ARRAY['Honda Wave 110'], 80.00, 15),
('หัวเทียน', 'SP-001', '8851234567892', ARRAY['Honda Wave 110', 'Yamaha Fino'], 45.00, 30);

INSERT INTO Services (Name, Description, BasePrice, Category) VALUES 
('เปลี่ยนน้ำมันเครื่อง', 'เปลี่ยนน้ำมันเครื่องและกรองน้ำมัน', 150.00, 'Maintenance'),
('ซ่อมเครื่อง', 'ตรวจเช็คและซ่อมแซมเครื่องยนต์', 500.00, 'Repair'),
('เปลี่ยนยาง', 'เปลี่ยนยางหน้าหรือยางหลัง', 200.00, 'Maintenance');
