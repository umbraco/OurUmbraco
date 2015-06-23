#Create tables script for SQL Server
	/* Form Picker */
	SET ANSI_NULLS ON
	GO
	SET QUOTED_IDENTIFIER ON
	GO
	IF NOT EXISTS (SELECT * FROM [cmsMacroPropertyType] WHERE macroPropertyTypeAlias = 'FormPicker')
	BEGIN
	INSERT INTO [cmsMacroPropertyType]
	           ([macroPropertyTypeAlias]
	           ,[macroPropertyTypeRenderAssembly]
	           ,[macroPropertyTypeRenderType]
	           ,[macroPropertyTypeBaseType])
	     VALUES
	           ('FormPicker'
	           ,'Umbraco.Forms.UI'
	           ,'MacroRenderings.FormPicker'
	           ,'String')
	END
	
	
	/* ---------------------AUTO GENERATED SQL BELOW THIS LINE */
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFWorkflowsToForms]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFWorkflowsToForms](
		[Workflow] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_UFWorkflowsToForms] PRIMARY KEY CLUSTERED 
	(
		[Workflow] ASC,
		[Form] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFSettings]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFSettings](
		[Id] [uniqueidentifier] NOT NULL,
		[Key] [nvarchar](250) NOT NULL,
		[Value] [nvarchar](2500) NULL
	 CONSTRAINT [PK_UFSettings] PRIMARY KEY CLUSTERED 
	(
		 [Id] ASC,
		 [Key] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFPrevalueSources]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFPrevalueSources](
		[Id] [uniqueidentifier] NOT NULL,
		[Name] [nvarchar](200) NULL,
		[Type] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_UFPrevalueProviders] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFUserSecurity]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFUserSecurity](
		[User] [nvarchar](50) NOT NULL,
		[ManageDataSources] [bit] NOT NULL,
		[ManagePreValueSources] [bit] NOT NULL,
		[ManageWorkflows] [bit] NOT NULL,
		[ManageForms] [bit] NOT NULL,
	 CONSTRAINT [PK_UFUserSecurity] PRIMARY KEY CLUSTERED 
	(
		[User] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFDataSources]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFDataSources](
		[Id] [uniqueidentifier] NOT NULL,
		[Type] [uniqueidentifier] NULL,
		[Name] [nvarchar](250) NULL,
	 CONSTRAINT [PK_UFDataSources] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFormStates]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFormStates](
		[State] [nvarchar](50) NOT NULL,
	 CONSTRAINT [PK_UFFormStates] PRIMARY KEY CLUSTERED 
	(
		[State] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFPrevalueSourceSettings]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFPrevalueSourceSettings](
		[PrevalueProvider] [uniqueidentifier] NOT NULL,
		[Key] [nvarchar](250) NOT NULL,
		[Value] [nvarchar](250) NULL,
	 CONSTRAINT [PK_UFPrevalueSettings] PRIMARY KEY CLUSTERED 
	(
		[PrevalueProvider] ASC,
		[Key] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFDataSourceMappings]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFDataSourceMappings](
		[DataSource] [uniqueidentifier] NOT NULL,
		[DataSourceField] [nvarchar](250) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[PrevalueTable] [nvarchar](250) NULL,
		[PrevalueKeyField] [nvarchar](250) NULL,
		[PrevalueValueField] [nvarchar](250) NULL,
		[DataType] [int] NULL,
		[DefaultValue] [nvarchar](max) NULL,
	 CONSTRAINT [PK_UFDataSourceMappings] PRIMARY KEY CLUSTERED 
	(
		[DataSource] ASC,
		[DataSourceField] ASC,
		[Form] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFForms]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFForms](
		[Id] [uniqueidentifier] NOT NULL,
		[Name] [nvarchar](100) NOT NULL,
		[Created] [datetime] NOT NULL CONSTRAINT [DF_UFForms_Created]  DEFAULT (getdate()),
		[ManualApproval] [bit] NOT NULL CONSTRAINT [DF_UFForms_ManualApproval]  DEFAULT ((0)),
		[GotoPageOnSubmit] [int] NOT NULL CONSTRAINT [DF_UFForms_GotoPageOnSubmit]  DEFAULT ((0)),
		[MessageOnSubmit] [nvarchar](500) NULL,
		[DataSource] [uniqueidentifier] NULL,
		[ShowValidationSummary] [bit] NOT NULL CONSTRAINT [DF_UFForms_ShowValidationSummary]  DEFAULT ((0)),
		[HideFieldValidation] [bit] NOT NULL CONSTRAINT [DF_UFForms_HideFieldValidation]  DEFAULT ((0)),
		[RequiredErrorMessage] [nvarchar](500) NULL,
		[InvalidErrorMessage] [nvarchar](500) NULL,
		[FieldIndicationType] [int] NULL,
		[Indicator] [nvarchar](500) NULL,
		[Archived] [bit] NOT NULL DEFAULT ((0)),
		[StoreRecordsLocally] [bit] NOT NULL DEFAULT ((1)),
		[DisableDefaultStylesheet] [bit] NOT NULL DEFAULT ((0)),
		[Entries] [int] NOT NULL CONSTRAINT [DF_UFForms_Entries]  DEFAULT ((0)),
		[Views] [int] NOT NULL CONSTRAINT [DF_UFForms_Views]  DEFAULT ((0)),
	 CONSTRAINT [PK_UFForms] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFWorkflowExecutionStates]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFWorkflowExecutionStates](
		[Workflow] [uniqueidentifier] NOT NULL,
		[State] [nvarchar](50) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_UFWorkflowExecutionStates] PRIMARY KEY CLUSTERED 
	(
		[Workflow] ASC,
		[State] ASC,
		[Form] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	) 
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFWorkflows]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFWorkflows](
		[Id] [uniqueidentifier] NOT NULL,
		[Name] [nvarchar](250) NOT NULL,
		[Type] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFWorkflows_SortOrder]  DEFAULT ((0)),
		[Active] [bit] NOT NULL CONSTRAINT [DF_UFWorkflows_Active]  DEFAULT ((0)),
		[Form] [uniqueidentifier] NOT NULL,
		[ExecutesOn] [int] NULL,
	 CONSTRAINT [PK_UFWorkflows] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	) 
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordsXml]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordsXml](
		[id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[xml] [ntext] NOT NULL,
		[created] [datetime] NOT NULL CONSTRAINT [DF_UFRecordsXml_created]  DEFAULT (getdate()),
		[Page] [int] NOT NULL CONSTRAINT [DF_UFRecordsXml_Page]  DEFAULT ((0)),
	 CONSTRAINT [PK_UFRecordsXml] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (IGNORE_DUP_KEY = OFF) 
	) 
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecords]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecords](
		[Id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[Created] [datetime] NOT NULL CONSTRAINT [DF_UFRecords_Created]  DEFAULT (getdate()),
		[Updated] [datetime] NOT NULL CONSTRAINT [DF_UFRecords_Updated]  DEFAULT (getdate()),
		[State] [int] NULL,
		[currentPage] [uniqueidentifier] NULL,
		[umbracoPageId] [int] NOT NULL CONSTRAINT [DF_UFRecords_umbracoPageId]  DEFAULT ((0)),
		[IP] [nvarchar](50) NULL,
		[MemberKey] [nvarchar](200) NULL,
	 CONSTRAINT [PK_UFRecords] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFUserFormSecurity]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFUserFormSecurity](
		[User] [nvarchar](50) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[HasAccess] [bit] NOT NULL,
		[SecurityType] [int] NOT NULL,
		[AllowInEditor] [bit] NOT NULL,
	 CONSTRAINT [PK_UFUserFormSecurity] PRIMARY KEY CLUSTERED 
	(
		[User] ASC,
		[Form] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFPages]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFPages](
		[Id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFPages_SortOrder]  DEFAULT ((0)),
		[Caption] [nvarchar](255) NOT NULL,
	 CONSTRAINT [PK_UFPages] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordFields]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordFields](
		[Key] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Record] [uniqueidentifier] NOT NULL,
		[DataType] [nvarchar](50) NOT NULL,
	 CONSTRAINT [PK_UFRecordFields] PRIMARY KEY CLUSTERED 
	(
		[Key] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFieldsets]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFieldsets](
		[Id] [uniqueidentifier] NOT NULL,
		[Page] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFFieldsets_SortOrder]  DEFAULT ((0)),
		[Caption] [nvarchar](250) NULL,
	 CONSTRAINT [PK_UFFieldsets] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFields]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFields](
		[Id] [uniqueidentifier] NOT NULL,
		[Fieldset] [uniqueidentifier] NOT NULL,
		[Fieldtype] [uniqueidentifier] NOT NULL,
		[PreValueProvider] [uniqueidentifier] NULL,
		[RegEx] [nvarchar](500) NULL,
		[Mandatory] [bit] NOT NULL CONSTRAINT [DF_UFFields_Mandatory]  DEFAULT ((0)),
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFFields_SortOrder]  DEFAULT ((0)),
		[Caption] [nvarchar](250) NOT NULL,
		[DataSourceField] [nvarchar](250) NULL,
		[ToolTip] [nvarchar](250) NULL,
		[DefaultValue] [nvarchar](250) NULL,
		[RequiredErrorMessage] [nvarchar](250) NULL,
		[InvalidErrorMessage] [nvarchar](250) NULL,
	 CONSTRAINT [PK_UFField] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFieldSettings]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFieldSettings](
		[Field] [uniqueidentifier] NULL,
		[Key] [nvarchar](250) NULL,
		[Value] [nvarchar](250) NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFPrevalues]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFPrevalues](
		[Id] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Value] [nvarchar](250) NOT NULL,
		[SortOrder] [int] NULL,
	 CONSTRAINT [PK_UFPrevalues] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordDataString]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordDataString](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [nvarchar](500) NOT NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordDataLongString]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordDataLongString](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [ntext] NOT NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordDataInteger]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordDataInteger](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [int] NOT NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordDataDateTime]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordDataDateTime](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [datetime] NOT NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFRecordDataBit]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFRecordDataBit](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [bit] NOT NULL
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFieldConditions]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFieldConditions](
		[Id] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Enabled] [bit] NOT NULL,
		[ActionType] [int] NULL,
		[LogicType] [int] NULL
	 CONSTRAINT [PK_UFFieldConditions] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	SET ANSI_NULLS ON
	
	SET QUOTED_IDENTIFIER ON
	
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UFFieldConditionRules]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [UFFieldConditionRules](
		[Id] [uniqueidentifier] NOT NULL,
		[FieldCondition] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Operator] [int] NOT NULL,
		[Value] [nvarchar](250) NULL
	 CONSTRAINT [PK_UFFieldConditionRules] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
	END
	
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFPrevalueSettings_UFPrevalueProviders]') AND parent_object_id = OBJECT_ID(N'[UFPrevalueSourceSettings]'))
	ALTER TABLE [UFPrevalueSourceSettings]  WITH CHECK ADD  CONSTRAINT [FK_UFPrevalueSettings_UFPrevalueProviders] FOREIGN KEY([PrevalueProvider])
	REFERENCES [UFPrevalueSources] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFPrevalueSourceSettings] CHECK CONSTRAINT [FK_UFPrevalueSettings_UFPrevalueProviders]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFDataSourceMappings_UFDataSources]') AND parent_object_id = OBJECT_ID(N'[UFDataSourceMappings]'))
	ALTER TABLE [UFDataSourceMappings]  WITH CHECK ADD  CONSTRAINT [FK_UFDataSourceMappings_UFDataSources] FOREIGN KEY([DataSource])
	REFERENCES [UFDataSources] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFDataSourceMappings] CHECK CONSTRAINT [FK_UFDataSourceMappings_UFDataSources]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFDataSourceMappings_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFDataSourceMappings]'))
	ALTER TABLE [UFDataSourceMappings]  WITH CHECK ADD  CONSTRAINT [FK_UFDataSourceMappings_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	
	ALTER TABLE [UFDataSourceMappings] CHECK CONSTRAINT [FK_UFDataSourceMappings_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFForms_UFDataSources]') AND parent_object_id = OBJECT_ID(N'[UFForms]'))
	ALTER TABLE [UFForms]  WITH CHECK ADD  CONSTRAINT [FK_UFForms_UFDataSources] FOREIGN KEY([DataSource])
	REFERENCES [UFDataSources] ([Id])
	ON DELETE SET NULL
	
	ALTER TABLE [UFForms] CHECK CONSTRAINT [FK_UFForms_UFDataSources]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFWorkflowExecutionStates_UFFormStates]') AND parent_object_id = OBJECT_ID(N'[UFWorkflowExecutionStates]'))
	ALTER TABLE [UFWorkflowExecutionStates]  WITH CHECK ADD  CONSTRAINT [FK_UFWorkflowExecutionStates_UFFormStates] FOREIGN KEY([State])
	REFERENCES [UFFormStates] ([State])
	
	ALTER TABLE [UFWorkflowExecutionStates] CHECK CONSTRAINT [FK_UFWorkflowExecutionStates_UFFormStates]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFWorkflows_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFWorkflows]'))
	ALTER TABLE [UFWorkflows]  WITH CHECK ADD  CONSTRAINT [FK_UFWorkflows_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFWorkflows] CHECK CONSTRAINT [FK_UFWorkflows_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordsXml_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFRecordsXml]'))
	ALTER TABLE [UFRecordsXml]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordsXml_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordsXml] CHECK CONSTRAINT [FK_UFRecordsXml_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecords_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFRecords]'))
	ALTER TABLE [UFRecords]  WITH CHECK ADD  CONSTRAINT [FK_UFRecords_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	
	ALTER TABLE [UFRecords] CHECK CONSTRAINT [FK_UFRecords_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFUserFormSecurity_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFUserFormSecurity]'))
	ALTER TABLE [UFUserFormSecurity]  WITH CHECK ADD  CONSTRAINT [FK_UFUserFormSecurity_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFUserFormSecurity] CHECK CONSTRAINT [FK_UFUserFormSecurity_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFPages_UFForms]') AND parent_object_id = OBJECT_ID(N'[UFPages]'))
	ALTER TABLE [UFPages]  WITH CHECK ADD  CONSTRAINT [FK_UFPages_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFPages] CHECK CONSTRAINT [FK_UFPages_UFForms]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordFields_UFFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordFields]'))
	ALTER TABLE [UFRecordFields]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordFields_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordFields] CHECK CONSTRAINT [FK_UFRecordFields_UFFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordFields_UFRecords]') AND parent_object_id = OBJECT_ID(N'[UFRecordFields]'))
	ALTER TABLE [UFRecordFields]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordFields_UFRecords] FOREIGN KEY([Record])
	REFERENCES [UFRecords] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordFields] CHECK CONSTRAINT [FK_UFRecordFields_UFRecords]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFFieldsets_UFPages]') AND parent_object_id = OBJECT_ID(N'[UFFieldsets]'))
	ALTER TABLE [UFFieldsets]  WITH CHECK ADD  CONSTRAINT [FK_UFFieldsets_UFPages] FOREIGN KEY([Page])
	REFERENCES [UFPages] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFFieldsets] CHECK CONSTRAINT [FK_UFFieldsets_UFPages]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFField_UFFieldsets]') AND parent_object_id = OBJECT_ID(N'[UFFields]'))
	ALTER TABLE [UFFields]  WITH CHECK ADD  CONSTRAINT [FK_UFField_UFFieldsets] FOREIGN KEY([Fieldset])
	REFERENCES [UFFieldsets] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFFields] CHECK CONSTRAINT [FK_UFField_UFFieldsets]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFFieldSettings_UFFields]') AND parent_object_id = OBJECT_ID(N'[UFFieldSettings]'))
	ALTER TABLE [UFFieldSettings]  WITH CHECK ADD  CONSTRAINT [FK_UFFieldSettings_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFFieldSettings] CHECK CONSTRAINT [FK_UFFieldSettings_UFFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFPrevalues_UFFields]') AND parent_object_id = OBJECT_ID(N'[UFPrevalues]'))
	ALTER TABLE [UFPrevalues]  WITH CHECK ADD  CONSTRAINT [FK_UFPrevalues_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE
	
	ALTER TABLE [UFPrevalues] CHECK CONSTRAINT [FK_UFPrevalues_UFFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordDataString_UFRecordFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordDataString]'))
	ALTER TABLE [UFRecordDataString]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordDataString_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordDataString] CHECK CONSTRAINT [FK_UFRecordDataString_UFRecordFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordDataLongString_UFRecordFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordDataLongString]'))
	ALTER TABLE [UFRecordDataLongString]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordDataLongString_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordDataLongString] CHECK CONSTRAINT [FK_UFRecordDataLongString_UFRecordFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordDataInteger_UFRecordFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordDataInteger]'))
	ALTER TABLE [UFRecordDataInteger]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordDataInteger_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordDataInteger] CHECK CONSTRAINT [FK_UFRecordDataInteger_UFRecordFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordDataDateTime_UFRecordFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordDataDateTime]'))
	ALTER TABLE [UFRecordDataDateTime]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordDataDateTime_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordDataDateTime] CHECK CONSTRAINT [FK_UFRecordDataDateTime_UFRecordFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFRecordDataBit_UFRecordFields]') AND parent_object_id = OBJECT_ID(N'[UFRecordDataBit]'))
	ALTER TABLE [UFRecordDataBit]  WITH CHECK ADD  CONSTRAINT [FK_UFRecordDataBit_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE
	
	ALTER TABLE [UFRecordDataBit] CHECK CONSTRAINT [FK_UFRecordDataBit_UFRecordFields]
	
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFFieldConditions_UFFields]') AND parent_object_id = OBJECT_ID(N'[UFFieldConditions]'))
	ALTER TABLE [UFFieldConditions]  WITH CHECK ADD  CONSTRAINT [FK_UFFieldConditions_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE
	
	
	ALTER TABLE [UFFieldConditions] CHECK CONSTRAINT [FK_UFFieldConditions_UFFields]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFFieldConditionRules_UFFieldConditions]') AND parent_object_id = OBJECT_ID(N'[UFFieldConditionRules]'))
	ALTER TABLE [UFFieldConditionRules]  WITH CHECK ADD  CONSTRAINT [FK_UFFieldConditionRules_UFFieldConditions] FOREIGN KEY([FieldCondition])
	REFERENCES [UFFieldConditions] ([Id])
	ON DELETE CASCADE
	
	
	ALTER TABLE [UFFieldConditionRules] CHECK CONSTRAINT [FK_UFFieldConditionRules_UFFieldConditions]
	
	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_UFFieldConditionRules_UFFields]') AND parent_object_id = OBJECT_ID(N'[UFFieldConditionRules]'))
	ALTER TABLE [UFFieldConditionRules]  WITH CHECK ADD  CONSTRAINT [FK_UFFieldConditionRules_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	
	
	ALTER TABLE [UFFieldConditionRules] CHECK CONSTRAINT [FK_UFFieldConditionRules_UFFields]
	