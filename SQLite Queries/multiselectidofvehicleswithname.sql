SELECT id, name
FROM vehicles 
WHERE name LIKE '%Supra%' 
   OR name LIKE '%Nissan%' AND active = '1';