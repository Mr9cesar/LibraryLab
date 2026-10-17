CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Resources` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Status` int NOT NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    `AvailabilityVersion` bigint NOT NULL,
    `Kind` int NOT NULL,
    CONSTRAINT `PK_Resources` PRIMARY KEY (`Id`),
    CONSTRAINT `AK_Resources_Id_Kind` UNIQUE (`Id`, `Kind`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(254) CHARACTER SET utf8mb4 NOT NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Books` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Isbn` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Author` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Books` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Books_Resources_Id` FOREIGN KEY (`Id`) REFERENCES `Resources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Equipment` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `InventoryCode` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Type` int NOT NULL,
    CONSTRAINT `PK_Equipment` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Equipment_Resources_Id` FOREIGN KEY (`Id`) REFERENCES `Resources` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Loans` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ResourceId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ResourceKind` int NOT NULL,
    `BorrowedAt` datetime(6) NOT NULL,
    `DueAt` datetime(6) NOT NULL,
    `ReturnedAt` datetime(6) NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_Loans` PRIMARY KEY (`Id`),
    CONSTRAINT `CK_Loans_DueAfterBorrowed` CHECK (DueAt > BorrowedAt),
    CONSTRAINT `CK_Loans_ResourceKind` CHECK (ResourceKind IN (1, 2)),
    CONSTRAINT `CK_Loans_ReturnState` CHECK (((Status = 1 AND ReturnedAt IS NULL) OR (Status = 2 AND ReturnedAt IS NOT NULL))),
    CONSTRAINT `FK_Loans_Resources_ResourceId_ResourceKind` FOREIGN KEY (`ResourceId`, `ResourceKind`) REFERENCES `Resources` (`Id`, `Kind`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Loans_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Books_Isbn` ON `Books` (`Isbn`);

CREATE UNIQUE INDEX `IX_Equipment_InventoryCode` ON `Equipment` (`InventoryCode`);

CREATE INDEX `IX_Loans_ResourceId_ResourceKind` ON `Loans` (`ResourceId`, `ResourceKind`);

CREATE INDEX `IX_Loans_ResourceId_Status` ON `Loans` (`ResourceId`, `Status`);

CREATE INDEX `IX_Loans_UserId_Status` ON `Loans` (`UserId`, `Status`);

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261003001904_InitialCreate', '8.0.13');

COMMIT;

