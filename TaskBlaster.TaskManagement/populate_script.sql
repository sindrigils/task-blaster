-- Insert into the Statuses table
INSERT INTO "Statuses" ("Id", "Name", "Description") VALUES 
(1, 'Active', 'Task is currently active'),
(2, 'Completed', 'Task has been completed'),
(3, 'Pending', 'Task is pending review'),
(4, 'Archived', 'Task is archived and not visible');

-- Insert into the Priorities table
INSERT INTO "Priorities" ("Id", "Name", "Description") VALUES 
(1, 'Low', 'Low priority task'),
(2, 'Medium', 'Medium priority task'),
(3, 'High', 'High priority task'),
(4, 'Urgent', 'Urgent task that requires immediate attention');
