/*======================================
		Alter DB exceptions tests
		[No changes expected]
======================================*/

USE `nz_test_closure`;

CALL before_alter_db_schema();
INSERT INTO `type_add_list`
(`name`, `table_name`, `base_type_name`)
VALUES
('Parent', '_parent', NULL),
('Child1', '_child1', 'Parent'),
('Child2', '_child2', 'Parent'),
('Child3', '_child3', 'Child1');
INSERT INTO `field_add_list`
(`owner_type_name`, `name`, `col_name`, `type_name`, `is_list`, `compare_options`, `back_ref_name`)
VALUES
('Child1', 'RightChild', '_right_child', 'Child2', FALSE, 'IgnoreCase', NULL);
CALL alter_db_schema();

/*---------------------------------------/
	Add type with incorrect parent
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `type_add_list`
(`name`, `table_name`, `base_type_name`)
VALUES
('Child4', '_child4', 'Parent'),
('NewType', '_new_type', 'IncorrectParent');	-- incorrect
CALL alter_db_schema();

/*---------------------------------------/
	Add type with existing table name
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `type_add_list`
(`name`, `table_name`, `base_type_name`)
VALUES
('Child4', '_child4', 'Parent'),
('NewType1', '_parent', 'Parent');	-- incorrect
CALL alter_db_schema();

/*---------------------------------------/
	Add field with incorrect types
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `field_add_list`
(`owner_type_name`, `name`, `col_name`, `type_name`, `is_list`, `compare_options`, `back_ref_name`)
VALUES
('Parent', 'Age', '_age', 'INT', FALSE, 'None', NULL),
('Parent', 'Name', '_name', 'IncorrectType123~', FALSE, 'IgnoreCase', NULL);	-- incorrect
CALL alter_db_schema();

/*---------------------------------------/
		Remove inexisting type
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `type_rem_list`
(`name`)
VALUES
('Child1'),
('InexistingType');	-- incorrect
CALL alter_db_schema();

/*---------------------------------------/
		Remove inherited type
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `type_rem_list`
(`name`)
VALUES
('Parent'),	-- inherited by Child1
('Child2');
CALL alter_db_schema();

/*---------------------------------------/
		Remove inexisting field
--------------------------------------*/
CALL before_alter_db_schema();
INSERT INTO `field_rem_list`
(`owner_type_name`, `name`)
VALUES
('Child1', 'RightChild'),
('Child1', 'InexistingFiled');
CALL alter_db_schema();

/*---------------------------------------/
		Add duplicated field
--------------------------------------*/
INSERT INTO `field_add_list`
(`owner_type_name`, `name`, `col_name`, `type_name`, `is_list`, `compare_options`, `back_ref_name`)
VALUES
('Child1', 'RightChild', '_right_child_new', 'Child3', FALSE, 'IgnoreCase', NULL);
CALL alter_db_schema();