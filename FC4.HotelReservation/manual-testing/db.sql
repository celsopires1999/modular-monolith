SELECT * from room_type_inventory;

SELECT id FROM payments ORDER BY processed_at DESC LIMIT 1;

SELECT * FROM payments ORDER BY processed_at;

SELECT * from event_store ORDER BY occurred_on;

SELECT * from event_store WHERE aggregate_id = '634a7492-24c9-42f7-b5c5-b7d62a944452' ORDER BY occurred_on;

