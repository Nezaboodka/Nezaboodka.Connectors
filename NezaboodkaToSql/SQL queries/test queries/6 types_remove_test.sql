/*********************************************

			Remove types tests
	(based on `Types ordering tests`
		and `Add types and fields tests`)

**********************************************/

USE `nz_admin_db`;

/*---------------------------------------/
		Remove full hierarchy
--------------------------------------*/
CALL before_alter_database_schema();
INSERT INTO `type_rem_list`
(`name`)
VALUES
('Moderator'),
('Admin'),
('UberAdmin'),
('Group'),
('User');
CALL alter_database_schema('nz_test_db');

/*---------------------------------------/
		Remove terminating types
--------------------------------------*/
CALL before_alter_database_schema();
INSERT INTO `type_rem_list`
(`name`)
VALUES
('CoolChopper'),
('Sedan'),
('HotRod');
CALL alter_database_schema('nz_test_db');

/*---------------------------------------/
		Remove referenced type
			[Error expected]
--------------------------------------*/
CALL before_alter_database_schema();
INSERT INTO `type_rem_list`
(`name`)
VALUES
('Car');	-- referenced by VeryGoodPeople
CALL alter_database_schema('nz_test_db');
