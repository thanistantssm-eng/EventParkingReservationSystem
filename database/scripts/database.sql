IF DB_ID('EventParkingReservationDb') IS NULL CREATE DATABASE EventParkingReservationDb;
GO
USE EventParkingReservationDb;
GO
CREATE TABLE Customers (Id int IDENTITY PRIMARY KEY, Name nvarchar(120) NOT NULL, Email nvarchar(256) NOT NULL UNIQUE, Phone nvarchar(30) NOT NULL);
CREATE TABLE Events (Id int IDENTITY PRIMARY KEY, Name nvarchar(160) NOT NULL, StartsAtUtc datetime2 NOT NULL, TicketPrice decimal(18,2) NOT NULL, ParkingFee decimal(18,2) NOT NULL);
CREATE TABLE Seats (Id int IDENTITY PRIMARY KEY, EventId int NOT NULL REFERENCES Events(Id), SeatNumber nvarchar(20) NOT NULL, CONSTRAINT UQ_Seats_Event UNIQUE(EventId, SeatNumber));
CREATE TABLE ParkingSlots (Id int IDENTITY PRIMARY KEY, EventId int NOT NULL REFERENCES Events(Id), SlotNumber nvarchar(20) NOT NULL, CONSTRAINT UQ_Parking_Event UNIQUE(EventId, SlotNumber));
CREATE TABLE Bookings (Id int IDENTITY PRIMARY KEY, BookingNumber nvarchar(40) NOT NULL UNIQUE, CustomerId int NOT NULL REFERENCES Customers(Id), EventId int NOT NULL REFERENCES Events(Id), Status nvarchar(30) NOT NULL, TotalAmount decimal(18,2) NOT NULL, CreatedAtUtc datetime2 NOT NULL, CancelledAtUtc datetime2 NULL);
CREATE TABLE BookingTickets (Id int IDENTITY PRIMARY KEY, BookingId int NOT NULL REFERENCES Bookings(Id), TicketType nvarchar(50) NOT NULL, Quantity int NOT NULL CHECK (Quantity > 0), UnitPrice decimal(18,2) NOT NULL);
CREATE TABLE BookingSeats (Id int IDENTITY PRIMARY KEY, BookingId int NOT NULL REFERENCES Bookings(Id), SeatId int NOT NULL UNIQUE REFERENCES Seats(Id));
CREATE TABLE BookingParkings (Id int IDENTITY PRIMARY KEY, BookingId int NOT NULL UNIQUE REFERENCES Bookings(Id), ParkingSlotId int NOT NULL UNIQUE REFERENCES ParkingSlots(Id), Fee decimal(18,2) NOT NULL);
CREATE TABLE Payments (Id int IDENTITY PRIMARY KEY, BookingId int NOT NULL UNIQUE REFERENCES Bookings(Id), Amount decimal(18,2) NOT NULL, Method nvarchar(30) NOT NULL, TransactionReference nvarchar(80) NOT NULL, Status nvarchar(30) NOT NULL, CreatedAtUtc datetime2 NOT NULL, CompletedAtUtc datetime2 NULL);
CREATE TABLE OtpVerifications (Id int IDENTITY PRIMARY KEY, PaymentId int NOT NULL UNIQUE REFERENCES Payments(Id), CodeHash char(64) NOT NULL, ExpiresAtUtc datetime2 NOT NULL, FailedAttempts int NOT NULL DEFAULT 0, VerifiedAtUtc datetime2 NULL);
CREATE TABLE QrCodes (Id int IDENTITY PRIMARY KEY, BookingId int NOT NULL UNIQUE REFERENCES Bookings(Id), Token nvarchar(100) NOT NULL UNIQUE, Payload nvarchar(500) NOT NULL, CreatedAtUtc datetime2 NOT NULL);
CREATE TABLE Notifications (Id int IDENTITY PRIMARY KEY, CustomerId int NOT NULL REFERENCES Customers(Id), BookingId int NULL REFERENCES Bookings(Id), Title nvarchar(160) NOT NULL, Message nvarchar(1000) NOT NULL, IsRead bit NOT NULL DEFAULT 0, CreatedAtUtc datetime2 NOT NULL);
GO
INSERT Customers(Name,Email,Phone) VALUES ('Demo Customer','demo@example.com','0770000000');
INSERT Events(Name,StartsAtUtc,TicketPrice,ParkingFee) VALUES ('Colombo Music Night',DATEADD(day,30,SYSUTCDATETIME()),2500,500);
DECLARE @EventId int=SCOPE_IDENTITY();
INSERT Seats(EventId,SeatNumber) VALUES (@EventId,'A1'),(@EventId,'A2'),(@EventId,'A3'),(@EventId,'A4'),(@EventId,'A5');
INSERT ParkingSlots(EventId,SlotNumber) VALUES (@EventId,'P1'),(@EventId,'P2'),(@EventId,'P3');
GO
