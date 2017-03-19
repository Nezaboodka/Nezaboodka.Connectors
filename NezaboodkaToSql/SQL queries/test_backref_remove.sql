/*********************************************

		Back references update test
		(using test_db_work schema)

**********************************************/

USE nz_admin_db;
CALL before_alter_database_schema();

INSERT INTO backref_upd_list
(field_owner_type_name, field_name, new_back_ref_name)
VALUES
('Admin', 'ControlGroup', NULL);

CALL alter_database_schema('nz_test_db');
