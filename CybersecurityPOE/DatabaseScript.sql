

CREATE DATABASE CybersecurityPOE;
GO

USE CybersecurityPOE;
GO

CREATE TABLE Tasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    ReminderDate DATETIME,
    IsCompleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

INSERT INTO Tasks (Title, Description, ReminderDate) VALUES 
('Enable Two-Factor Authentication', 'Add 2FA to your email and banking accounts', DATEADD(DAY, 7, GETDATE())),
('Review Privacy Settings', 'Check social media privacy settings', DATEADD(DAY, 3, GETDATE())),
('Update Passwords', 'Change passwords for critical accounts', DATEADD(DAY, 14, GETDATE()));
GO

SELECT * FROM Tasks;
GO
