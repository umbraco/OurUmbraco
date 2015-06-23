#Create tables script for MySQL

	INSERT INTO cmsMacroPropertyType(macroPropertyTypeAlias,macroPropertyTypeRenderAssembly,macroPropertyTypeRenderType,macroPropertyTypeBaseType)  VALUES ('FormPicker','Umbraco.Forms.UI','MacroRenderings.FormPicker','String');
	SET FOREIGN_KEY_CHECKS = 0;
	CREATE TABLE `UFWorkflowsToForms` (
	  `Workflow` VARCHAR(64) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  PRIMARY KEY (`Workflow`, `Form`)
	);
	CREATE TABLE `UFSettings` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Key` VARCHAR(250) NOT NULL,
	  `Value` VARCHAR(2500) NULL
	);
	CREATE TABLE `UFPrevalueSources` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Name` VARCHAR(200) NULL,
	  `Type` VARCHAR(64) NULL,
	  PRIMARY KEY (`Id`)
	);
	CREATE TABLE `UFUserSecurity` (
	  `User` VARCHAR(50) NOT NULL,
	  `ManageDataSources` TINYINT NOT NULL,
	  `ManagePreValueSources` TINYINT NOT NULL,
	  `ManageWorkflows` TINYINT NOT NULL,
	  `ManageForms` TINYINT NOT NULL,
	  PRIMARY KEY (`User`)
	);
	CREATE TABLE `UFDataSources` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Type` VARCHAR(64) NULL,
	  `Name` VARCHAR(250) NULL,
	  PRIMARY KEY (`Id`)
	);
	CREATE TABLE `UFFormStates` (
	  `State` VARCHAR(50) NOT NULL,
	  PRIMARY KEY (`State`)
	);
	CREATE TABLE `UFPrevalueSourceSettings` (
	  `PrevalueProvider` VARCHAR(64) NOT NULL,
	  `Key` VARCHAR(250) NOT NULL,
	  `Value` VARCHAR(250) NULL,
	  PRIMARY KEY (`PrevalueProvider`, `Key`),
	  CONSTRAINT `FK_UFPrevalueSettings_UFPrevalueProviders` FOREIGN KEY `FK_UFPrevalueSettings_UFPrevalueProviders` (`PrevalueProvider`)
	    REFERENCES `UFPrevalueSources` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFForms` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Name` VARCHAR(100) NOT NULL,
	  `Created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	  `ManualApproval` TINYINT NOT NULL DEFAULT 0,
	  `GotoPageOnSubmit` INT(10) NOT NULL DEFAULT 0,
	  `MessageOnSubmit` VARCHAR(500) NULL,
	  `DataSource` VARCHAR(64) NULL,
	  `ShowValidationSummary` TINYINT NOT NULL DEFAULT 0,
	  `HideFieldValidation` TINYINT NOT NULL DEFAULT 0,
	  `RequiredErrorMessage` VARCHAR(500) NULL,
	  `InvalidErrorMessage` VARCHAR(500) NULL,
	  `FieldIndicationType` INT(10) NULL,
	  `Indicator` VARCHAR(500) NULL,
	  `Archived` TINYINT NOT NULL DEFAULT 0,
	  `StoreRecordsLocally` TINYINT NOT NULL DEFAULT 1,
	  `DisableDefaultStylesheet` TINYINT NOT NULL DEFAULT 0,
	  `Entries` INT(10) NOT NULL DEFAULT 0,
	  `Views` INT(10) NOT NULL DEFAULT 0,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFForms_UFDataSources` FOREIGN KEY `FK_UFForms_UFDataSources` (`DataSource`)
	    REFERENCES `UFDataSources` (`Id`)
	    ON DELETE NO ACTION
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFDataSourceMappings` (
	  `DataSource` VARCHAR(64) NOT NULL,
	  `DataSourceField` VARCHAR(250) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  `PrevalueTable` VARCHAR(250) NULL,
	  `PrevalueKeyField` VARCHAR(250) NULL,
	  `PrevalueValueField` VARCHAR(250) NULL,
	  `DataType` INT(10) NULL,
	  `DefaultValue` VARCHAR(500) NULL,
	  PRIMARY KEY (`DataSource`, `DataSourceField`, `Form`),
	  CONSTRAINT `FK_UFDataSourceMappings_UFDataSources` FOREIGN KEY `FK_UFDataSourceMappings_UFDataSources` (`DataSource`)
	    REFERENCES `UFDataSources` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION,
	  CONSTRAINT `FK_UFDataSourceMappings_UFForms` FOREIGN KEY `FK_UFDataSourceMappings_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE NO ACTION
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFWorkflowExecutionStates` (
	  `Workflow` VARCHAR(64) NOT NULL,
	  `State` VARCHAR(50) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  PRIMARY KEY (`Workflow`, `State`, `Form`),
	  CONSTRAINT `FK_UFWorkflowExecutionStates_UFFormStates` FOREIGN KEY `FK_UFWorkflowExecutionStates_UFFormStates` (`State`)
	    REFERENCES `UFFormStates` (`State`)
	    ON DELETE NO ACTION
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFWorkflows` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Name` VARCHAR(250) NOT NULL,
	  `Type` VARCHAR(64) NOT NULL,
	  `SortOrder` INT(10) NOT NULL DEFAULT 0,
	  `Active` TINYINT NOT NULL DEFAULT 0,
	  `Form` VARCHAR(64) NOT NULL,
	  `ExecutesOn` INT(10) NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFWorkflows_UFForms` FOREIGN KEY `FK_UFWorkflows_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordsXml` (
	  `id` VARCHAR(64) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  `xml` LONGTEXT NOT NULL,
	  `created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	  `Page` INT(10) NOT NULL DEFAULT 0,
	  PRIMARY KEY (`id`),
	  CONSTRAINT `FK_UFRecordsXml_UFForms` FOREIGN KEY `FK_UFRecordsXml_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecords` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  `Created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	  `Updated` TIMESTAMP NULL,
	  `State` INT(10) NULL,
	  `currentPage` VARCHAR(64) NULL,
	  `umbracoPageId` INT(10) NOT NULL DEFAULT 0,
	  `IP` VARCHAR(50) NULL,
	  `MemberKey` VARCHAR(200) NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFRecords_UFForms` FOREIGN KEY `FK_UFRecords_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE NO ACTION
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFUserFormSecurity` (
	  `User` VARCHAR(50) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  `HasAccess` TINYINT NOT NULL,
	  `SecurityType` INT(10) NOT NULL,
	  `AllowInEditor` TINYINT NOT NULL,
	  PRIMARY KEY (`User`, `Form`),
	  CONSTRAINT `FK_UFUserFormSecurity_UFForms` FOREIGN KEY `FK_UFUserFormSecurity_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFPages` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Form` VARCHAR(64) NOT NULL,
	  `SortOrder` INT(10) NOT NULL DEFAULT 0,
	  `Caption` VARCHAR(255) NOT NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFPages_UFForms` FOREIGN KEY `FK_UFPages_UFForms` (`Form`)
	    REFERENCES `UFForms` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFFieldsets` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Page` VARCHAR(64) NOT NULL,
	  `SortOrder` INT(10) NOT NULL DEFAULT 0,
	  `Caption` VARCHAR(250) NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFFieldsets_UFPages` FOREIGN KEY `FK_UFFieldsets_UFPages` (`Page`)
	    REFERENCES `UFPages` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFFields` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Fieldset` VARCHAR(64) NOT NULL,
	  `Fieldtype` VARCHAR(64) NOT NULL,
	  `PreValueProvider` VARCHAR(64) NULL,
	  `RegEx` VARCHAR(500) NULL,
	  `Mandatory` TINYINT NOT NULL DEFAULT 0,
	  `SortOrder` INT(10) NOT NULL DEFAULT 0,
	  `Caption` VARCHAR(250) NOT NULL,
	  `DataSourceField` VARCHAR(250) NULL,
	  `ToolTip` VARCHAR(250) NULL,
	  `DefaultValue` VARCHAR(250) NULL,
	  `RequiredErrorMessage` VARCHAR(250) NULL,
	  `InvalidErrorMessage` VARCHAR(250) NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFField_UFFieldsets` FOREIGN KEY `FK_UFField_UFFieldsets` (`Fieldset`)
	    REFERENCES `UFFieldsets` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordFields` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Field` VARCHAR(64) NOT NULL,
	  `Record` VARCHAR(64) NOT NULL,
	  `DataType` VARCHAR(50) NOT NULL,
	  PRIMARY KEY (`Key`),
	  CONSTRAINT `FK_UFRecordFields_UFFields` FOREIGN KEY `FK_UFRecordFields_UFFields` (`Field`)
	    REFERENCES `UFFields` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION,
	  CONSTRAINT `FK_UFRecordFields_UFRecords` FOREIGN KEY `FK_UFRecordFields_UFRecords` (`Record`)
	    REFERENCES `UFRecords` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFFieldSettings` (
	  `Field` VARCHAR(64) NULL,
	  `Key` VARCHAR(250) NULL,
	  `Value` VARCHAR(250) NULL,
	  CONSTRAINT `FK_UFFieldSettings_UFFields` FOREIGN KEY `FK_UFFieldSettings_UFFields` (`Field`)
	    REFERENCES `UFFields` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFPrevalues` (
	  `Id` VARCHAR(64) NOT NULL,
	  `Field` VARCHAR(64) NOT NULL,
	  `Value` VARCHAR(250) NOT NULL,
	  `SortOrder` INT(10) NULL,
	  PRIMARY KEY (`Id`),
	  CONSTRAINT `FK_UFPrevalues_UFFields` FOREIGN KEY `FK_UFPrevalues_UFFields` (`Field`)
	    REFERENCES `UFFields` (`Id`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordDataString` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Value` VARCHAR(500) NOT NULL,
	  CONSTRAINT `FK_UFRecordDataString_UFRecordFields` FOREIGN KEY `FK_UFRecordDataString_UFRecordFields` (`Key`)
	    REFERENCES `UFRecordFields` (`Key`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordDataLongString` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Value` LONGTEXT NOT NULL,
	  CONSTRAINT `FK_UFRecordDataLongString_UFRecordFields` FOREIGN KEY `FK_UFRecordDataLongString_UFRecordFields` (`Key`)
	    REFERENCES `UFRecordFields` (`Key`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordDataInteger` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Value` INT(10) NOT NULL,
	  CONSTRAINT `FK_UFRecordDataInteger_UFRecordFields` FOREIGN KEY `FK_UFRecordDataInteger_UFRecordFields` (`Key`)
	    REFERENCES `UFRecordFields` (`Key`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordDataDateTime` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Value` DATETIME NOT NULL,
	  CONSTRAINT `FK_UFRecordDataDateTime_UFRecordFields` FOREIGN KEY `FK_UFRecordDataDateTime_UFRecordFields` (`Key`)
	    REFERENCES `UFRecordFields` (`Key`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	CREATE TABLE `UFRecordDataBit` (
	  `Key` VARCHAR(64) NOT NULL,
	  `Value` TINYINT NOT NULL,
	  CONSTRAINT `FK_UFRecordDataBit_UFRecordFields` FOREIGN KEY `FK_UFRecordDataBit_UFRecordFields` (`Key`)
	    REFERENCES `UFRecordFields` (`Key`)
	    ON DELETE CASCADE
	    ON UPDATE NO ACTION
	);
	
	CREATE TABLE `UFFieldConditions`(
		`Id` VARCHAR(64) NOT NULL,
		`Field` VARCHAR(64) NOT NULL,
		`Enabled` TINYINT NOT NULL,
		`ActionType` INT(10) NULL,
		`LogicType` INT(10) NULL,
		PRIMARY KEY (`Id`),
		CONSTRAINT `FK_UFFieldConditions_UFFields` FOREIGN KEY `FK_UFFieldConditions_UFFields` (`Field`)
			REFERENCES `UFFields` (`Id`)
			ON DELETE CASCADE
			ON UPDATE NO ACTION
	);
	
	CREATE TABLE `UFFieldConditionRules`(
		`Id` VARCHAR(64) NOT NULL,
		`FieldCondition` VARCHAR(64) NOT NULL,
		`Field` VARCHAR(64) NOT NULL,
		`Operator` TINYINT NOT NULL,
		`Value` VARCHAR(250) NULL,
		PRIMARY KEY (`Id`),
		CONSTRAINT `FK_UFFieldConditionRules_UFFieldConditions` FOREIGN KEY `FK_UFFieldConditionRules_UFFieldConditions` (`FieldCondition`)
			REFERENCES `UFFieldConditionsUFFields` (`Id`)
			ON DELETE CASCADE
			ON UPDATE NO ACTION
	);
	
	SET FOREIGN_KEY_CHECKS = 1