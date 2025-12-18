-- Seed Data for TaskManager Development Database
-- Run this script to populate the database with test data

USE TaskMgrDb;
GO

-- Insert Test Users
INSERT INTO Users (Id, Email, DisplayName, AzureObjectId, IsActive, CreatedAt, UpdatedAt)
VALUES
    (NEWID(), 'john.doe@example.com', 'John Doe', 'dev-azure-id-1', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'jane.smith@example.com', 'Jane Smith', 'dev-azure-id-2', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'bob.wilson@example.com', 'Bob Wilson', 'dev-azure-id-3', 1, GETUTCDATE(), GETUTCDATE());
GO

-- Get the user IDs for foreign key relationships
DECLARE @JohnId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'john.doe@example.com');
DECLARE @JaneId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'jane.smith@example.com');
DECLARE @BobId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'bob.wilson@example.com');

-- Insert Test Projects
DECLARE @Project1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Project2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Project3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Project4Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Projects (Id, Name, Description, OwnerUserId, Color, IsArchived, DueDate, CompletedAt, CreatedAt, UpdatedAt)
VALUES
    (@Project1Id, 'Website Redesign', 'Complete redesign of company website with modern UI/UX', @JohnId, '#3B82F6', 0, DATEADD(day, 30, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE()),
    (@Project2Id, 'Mobile App Development', 'Develop iOS and Android mobile applications', @JaneId, '#10B981', 0, DATEADD(day, 60, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE()),
    (@Project3Id, 'Marketing Campaign Q1', 'Q1 marketing campaign planning and execution', @BobId, '#F59E0B', 0, DATEADD(day, 15, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE()),
    (@Project4Id, 'Infrastructure Migration', 'Migrate infrastructure to cloud platform', @JohnId, '#8B5CF6', 0, DATEADD(day, 90, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE());
GO

-- Get project IDs
DECLARE @Project1Id UNIQUEIDENTIFIER = (SELECT Id FROM Projects WHERE Name = 'Website Redesign');
DECLARE @Project2Id UNIQUEIDENTIFIER = (SELECT Id FROM Projects WHERE Name = 'Mobile App Development');
DECLARE @Project3Id UNIQUEIDENTIFIER = (SELECT Id FROM Projects WHERE Name = 'Marketing Campaign Q1');
DECLARE @Project4Id UNIQUEIDENTIFIER = (SELECT Id FROM Projects WHERE Name = 'Infrastructure Migration');
DECLARE @JohnId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'john.doe@example.com');
DECLARE @JaneId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'jane.smith@example.com');
DECLARE @BobId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'bob.wilson@example.com');

-- Insert Test Tasks for Website Redesign
INSERT INTO Tasks (Id, Title, Description, Status, Priority, ProjectId, AssignedToUserId, DueDate, CompletedAt, EstimatedHours, ActualHours, Tags, CreatedAt, UpdatedAt)
VALUES
    -- Website Redesign Tasks
    (NEWID(), 'Design Homepage Mockup', 'Create initial homepage design mockup in Figma', 2, 1, @Project1Id, @JaneId, DATEADD(day, 5, GETUTCDATE()), NULL, 8, NULL, 'design,ui,homepage', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Implement Navigation Component', 'Build responsive navigation component with React', 1, 1, @Project1Id, @JohnId, DATEADD(day, 7, GETUTCDATE()), NULL, 12, 6, 'frontend,react,component', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Setup CI/CD Pipeline', 'Configure automated deployment pipeline', 0, 2, @Project1Id, @BobId, DATEADD(day, 10, GETUTCDATE()), NULL, 6, NULL, 'devops,automation', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Write Unit Tests', 'Add unit tests for all components', 0, 2, @Project1Id, @JohnId, DATEADD(day, 15, GETUTCDATE()), NULL, 16, NULL, 'testing,quality', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Performance Optimization', 'Optimize page load times and bundle size', 0, 1, @Project1Id, NULL, DATEADD(day, 20, GETUTCDATE()), NULL, 10, NULL, 'performance,optimization', GETUTCDATE(), GETUTCDATE()),

    -- Mobile App Development Tasks
    (NEWID(), 'Setup React Native Project', 'Initialize React Native project with TypeScript', 3, 1, @Project2Id, @JaneId, DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -4, GETUTCDATE()), 4, 5, 'mobile,setup,react-native', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Design App Screens', 'Design all main app screens in Figma', 2, 1, @Project2Id, @JaneId, DATEADD(day, 8, GETUTCDATE()), NULL, 20, 10, 'design,ui,mobile', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Implement Authentication', 'Add user authentication with OAuth', 1, 0, @Project2Id, @BobId, DATEADD(day, 12, GETUTCDATE()), NULL, 16, 8, 'auth,security,backend', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Build API Integration', 'Connect mobile app to backend API', 0, 1, @Project2Id, @JohnId, DATEADD(day, 15, GETUTCDATE()), NULL, 12, NULL, 'api,integration', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'App Store Submission', 'Prepare and submit apps to stores', 0, 2, @Project2Id, NULL, DATEADD(day, 50, GETUTCDATE()), NULL, 8, NULL, 'deployment,stores', GETUTCDATE(), GETUTCDATE()),

    -- Marketing Campaign Tasks
    (NEWID(), 'Market Research', 'Conduct competitor analysis and market research', 3, 1, @Project3Id, @BobId, DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -8, GETUTCDATE()), 16, 18, 'research,analysis', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Create Social Media Content', 'Design graphics and write copy for social posts', 2, 1, @Project3Id, @JaneId, DATEADD(day, 5, GETUTCDATE()), NULL, 20, 12, 'content,social-media,design', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Email Campaign Setup', 'Configure email marketing automation', 1, 1, @Project3Id, @BobId, DATEADD(day, 8, GETUTCDATE()), NULL, 8, 4, 'email,marketing,automation', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Launch Campaign', 'Go live with marketing campaign', 0, 0, @Project3Id, NULL, DATEADD(day, 12, GETUTCDATE()), NULL, 4, NULL, 'launch,execution', GETUTCDATE(), GETUTCDATE()),

    -- Infrastructure Migration Tasks
    (NEWID(), 'Cloud Architecture Design', 'Design scalable cloud architecture', 1, 0, @Project4Id, @JohnId, DATEADD(day, 10, GETUTCDATE()), NULL, 24, 10, 'architecture,cloud,design', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Database Migration Plan', 'Create detailed database migration strategy', 0, 0, @Project4Id, @BobId, DATEADD(day, 15, GETUTCDATE()), NULL, 16, NULL, 'database,migration,planning', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Security Audit', 'Perform comprehensive security assessment', 0, 0, @Project4Id, NULL, DATEADD(day, 20, GETUTCDATE()), NULL, 20, NULL, 'security,audit,compliance', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Load Testing', 'Test system under high load conditions', 0, 1, @Project4Id, @JohnId, DATEADD(day, 30, GETUTCDATE()), NULL, 12, NULL, 'testing,performance,load', GETUTCDATE(), GETUTCDATE());
GO

PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '- 3 Users created';
PRINT '- 4 Projects created';
PRINT '- 18 Tasks created';
PRINT '';
PRINT 'Test Users:';
PRINT '  - john.doe@example.com (John Doe)';
PRINT '  - jane.smith@example.com (Jane Smith)';
PRINT '  - bob.wilson@example.com (Bob Wilson)';
GO
