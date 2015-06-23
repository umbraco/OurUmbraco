##Create tables script for SQL CE

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
	;
	
	CREATE TABLE [UFWorkflowsToForms]
	(
		[Workflow] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL
	) 
	;
	
	ALTER TABLE [UFWorkflowsToForms] ADD CONSTRAINT [PK_UFWorkflowsToForms] PRIMARY KEY ([Workflow],[Form])
	;
	
	CREATE TABLE [UFSettings](
		[Id] [uniqueidentifier] NOT NULL,
		[Key] [nvarchar](250) NOT NULL,
		[Value] [nvarchar](1000) NULL
	)
	;
	
	CREATE TABLE [UFPrevalueSources](
		[Id] [uniqueidentifier] NOT NULL,
		[Name] [nvarchar](200) NULL,
		[Type] [uniqueidentifier] NULL
	  );
	
	ALTER TABLE [UFPrevalueSources] ADD CONSTRAINT [PK_UFPrevalueProviders] PRIMARY KEY ([Id])
	;
	  
	CREATE TABLE [UFUserSecurity](
		[User] [nvarchar](50) NOT NULL,
		[ManageDataSources] [bit] NOT NULL,
		[ManagePreValueSources] [bit] NOT NULL,
		[ManageWorkflows] [bit] NOT NULL,
		[ManageForms] [bit] NOT NULL
	);
	
	ALTER TABLE [UFUserSecurity] ADD CONSTRAINT [PK_UFUserSecurity] PRIMARY KEY ([User])
	;
	
	
	CREATE TABLE [UFDataSources](
		[Id] [uniqueidentifier] NOT NULL,
		[Type] [uniqueidentifier] NULL,
		[Name] [nvarchar](250) NULL
	);
	
	ALTER TABLE [UFDataSources] ADD CONSTRAINT [PK_UFDataSources] PRIMARY KEY ([Id])
	;
	
	
	CREATE TABLE [UFFormStates](
		[State] [nvarchar](50) NOT NULL
	 );
	
	 ALTER TABLE [UFFormStates] ADD CONSTRAINT [PK_UFFormStates] PRIMARY KEY ([State])
	;
	
	CREATE TABLE [UFPrevalueSourceSettings](
		[PrevalueProvider] [uniqueidentifier] NOT NULL,
		[Key] [nvarchar](250) NOT NULL,
		[Value] [nvarchar](250) NULL
	);
	
	ALTER TABLE [UFPrevalueSourceSettings] ADD CONSTRAINT [PK_UFPrevalueSettings] PRIMARY KEY ([PrevalueProvider],[Key]);
	
	
	
	CREATE TABLE [UFDataSourceMappings](
		[DataSource] [uniqueidentifier] NOT NULL,
		[DataSourceField] [nvarchar](250) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[PrevalueTable] [nvarchar](250) NULL,
		[PrevalueKeyField] [nvarchar](250) NULL,
		[PrevalueValueField] [nvarchar](250) NULL,
		[DataType] [int] NULL,
		[DefaultValue] [nvarchar](1000) NULL
	    );
	
	 ALTER TABLE [UFDataSourceMappings] ADD CONSTRAINT [PK_UFDataSourceMappings] PRIMARY KEY ([DataSource],[DataSourceField],[Form])
	;
	
	
	
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
		[Views] [int] NOT NULL CONSTRAINT [DF_UFForms_Views]  DEFAULT ((0))
	    );
	
	
		
	 ALTER TABLE [UFForms] ADD CONSTRAINT [PK_UFForms] PRIMARY KEY ([Id])
	;
	
	
	CREATE TABLE [UFWorkflowExecutionStates](
		[Workflow] [uniqueidentifier] NOT NULL,
		[State] [nvarchar](50) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL
	    );
	
	 ALTER TABLE [UFWorkflowExecutionStates] ADD CONSTRAINT [PK_UFWorkflowExecutionStates] PRIMARY KEY ([Workflow],[State],[Form])
	;
	
	CREATE TABLE [UFWorkflows](
		[Id] [uniqueidentifier] NOT NULL,
		[Name] [nvarchar](250) NOT NULL,
		[Type] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFWorkflows_SortOrder]  DEFAULT ((0)),
		[Active] [bit] NOT NULL CONSTRAINT [DF_UFWorkflows_Active]  DEFAULT ((0)),
		[Form] [uniqueidentifier] NOT NULL,
		[ExecutesOn] [int] NULL
	    );
	
	 ALTER TABLE [UFWorkflows] ADD CONSTRAINT [PK_UFWorkflows] PRIMARY KEY ([Id])
	;
	
	
	CREATE TABLE [UFRecordsXml](
		[id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[xml] [ntext] NOT NULL,
		[created] [datetime] NOT NULL CONSTRAINT [DF_UFRecordsXml_created]  DEFAULT (getdate()),
		[Page] [int] NOT NULL CONSTRAINT [DF_UFRecordsXml_Page]  DEFAULT ((0))
	    );
	
	 ALTER TABLE [UFRecordsXml] ADD CONSTRAINT [PK_UFRecordsXml] PRIMARY KEY ([Id])
	;
	    
	
	CREATE TABLE [UFRecords](
		[Id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[Created] [datetime] NOT NULL CONSTRAINT [DF_UFRecords_Created]  DEFAULT (getdate()),
		[Updated] [datetime] NOT NULL CONSTRAINT [DF_UFRecords_Updated]  DEFAULT (getdate()),
		[State] [int] NULL,
		[currentPage] [uniqueidentifier] NULL,
		[umbracoPageId] [int] NOT NULL CONSTRAINT [DF_UFRecords_umbracoPageId]  DEFAULT ((0)),
		[IP] [nvarchar](50) NULL,
		[MemberKey] [nvarchar](200) NULL
	    );
	
	
	 ALTER TABLE [UFRecords] ADD CONSTRAINT [PK_UFRecords] PRIMARY KEY ([Id])
	;
	    
	
	CREATE TABLE [UFUserFormSecurity](
		[User] [nvarchar](50) NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[HasAccess] [bit] NOT NULL,
		[SecurityType] [int] NOT NULL,
		[AllowInEditor] [bit] NOT NULL
	    );
	    
	
	 ALTER TABLE [UFUserFormSecurity] ADD CONSTRAINT [PK_UFUserFormSecurity] PRIMARY KEY ([User],[Form])
	;
	
	CREATE TABLE [UFPages](
		[Id] [uniqueidentifier] NOT NULL,
		[Form] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFPages_SortOrder]  DEFAULT ((0)),
		[Caption] [nvarchar](255) NOT NULL);
	
	ALTER TABLE [UFPages] ADD CONSTRAINT [PK_UFPages] PRIMARY KEY ([Id])
	;
	
	
	CREATE TABLE [UFRecordFields](
		[Key] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Record] [uniqueidentifier] NOT NULL,
		[DataType] [nvarchar](50) NOT NULL
	    );
	
	ALTER TABLE [UFRecordFields] ADD CONSTRAINT [PK_UFRecordFields] PRIMARY KEY ([Key]);
	
	CREATE TABLE [UFFieldsets](
		[Id] [uniqueidentifier] NOT NULL,
		[Page] [uniqueidentifier] NOT NULL,
		[SortOrder] [int] NOT NULL CONSTRAINT [DF_UFFieldsets_SortOrder]  DEFAULT ((0)),
		[Caption] [nvarchar](250) NULL
	    );
	    
	ALTER TABLE [UFFieldsets] ADD CONSTRAINT [PK_UFFieldsets] PRIMARY KEY ([Id])
	;
	
	
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
		[InvalidErrorMessage] [nvarchar](250) NULL
	    );
	
	ALTER TABLE [UFFields] ADD CONSTRAINT [PK_UFField] PRIMARY KEY ([Id])
	;
	
	
	CREATE TABLE [UFFieldSettings](
		[Field] [uniqueidentifier] NULL,
		[Key] [nvarchar](250) NULL,
		[Value] [nvarchar](250) NULL
	);
	
	
	CREATE TABLE [UFPrevalues](
		[Id] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Value] [nvarchar](250) NOT NULL,
		[SortOrder] [int] NULL)
	    ;
	
	ALTER TABLE [UFPrevalues] ADD CONSTRAINT [PK_UFPrevalues] PRIMARY KEY ([Id])
	;
	
	CREATE TABLE [UFRecordDataString](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [nvarchar](500) NOT NULL
	);
	
	
	CREATE TABLE [UFRecordDataLongString](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [ntext] NOT NULL
	);
	
	
	
	CREATE TABLE [UFRecordDataInteger](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [int] NOT NULL
	) ;
	
	
	CREATE TABLE [UFRecordDataDateTime](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [datetime] NOT NULL
	) ;
	
	CREATE TABLE [UFRecordDataBit](
		[Key] [uniqueidentifier] NOT NULL,
		[Value] [bit] NOT NULL
	) ;
	
	CREATE TABLE [UFFieldConditions](
		[Id] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Enabled] [bit] NOT NULL,
		[ActionType] [int] NULL,
		[LogicType] [int] NULL
	);
	
	ALTER TABLE [UFFieldConditions] ADD CONSTRAINT [PK_UFFieldConditions] PRIMARY KEY ([Id]);
	
	CREATE TABLE [UFFieldConditionRules](
		[Id] [uniqueidentifier] NOT NULL,
		[FieldCondition] [uniqueidentifier] NOT NULL,
		[Field] [uniqueidentifier] NOT NULL,
		[Operator] [int] NOT NULL,
		[Value] [nvarchar](250) NULL
	);
	
	ALTER TABLE [UFFieldConditionRules] ADD CONSTRAINT [PK_UFFieldConditionRules] PRIMARY KEY ([Id]);
	
	
	ALTER TABLE [UFPrevalueSourceSettings]  ADD  CONSTRAINT [FK_UFPrevalueSettings_UFPrevalueProviders] FOREIGN KEY([PrevalueProvider])
	REFERENCES [UFPrevalueSources] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFDataSourceMappings]  ADD  CONSTRAINT [FK_UFDataSourceMappings_UFDataSources] FOREIGN KEY([DataSource])
	REFERENCES [UFDataSources] ([Id])
	ON DELETE CASCADE;
	
	
	
	ALTER TABLE [UFDataSourceMappings] ADD  CONSTRAINT [FK_UFDataSourceMappings_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id]);
	
	
	ALTER TABLE [UFForms]   ADD  CONSTRAINT [FK_UFForms_UFDataSources] FOREIGN KEY([DataSource])
	REFERENCES [UFDataSources] ([Id])
	ON DELETE SET NULL;
	
	
	ALTER TABLE [UFWorkflowExecutionStates]  ADD  CONSTRAINT [FK_UFWorkflowExecutionStates_UFFormStates] FOREIGN KEY([State])
	REFERENCES [UFFormStates] ([State]);
	
	
	
	ALTER TABLE [UFWorkflows]  ADD  CONSTRAINT [FK_UFWorkflows_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE;
	
	
	ALTER TABLE [UFRecordsXml] ADD  CONSTRAINT [FK_UFRecordsXml_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE;
	
	
	ALTER TABLE [UFRecords] ADD  CONSTRAINT [FK_UFRecords_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id]);
	
	
	
	ALTER TABLE [UFUserFormSecurity]  ADD  CONSTRAINT [FK_UFUserFormSecurity_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFPages] ADD  CONSTRAINT [FK_UFPages_UFForms] FOREIGN KEY([Form])
	REFERENCES [UFForms] ([Id])
	ON DELETE CASCADE;
	
	
	ALTER TABLE [UFRecordFields] ADD  CONSTRAINT [FK_UFRecordFields_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFRecordFields]  ADD  CONSTRAINT [FK_UFRecordFields_UFRecords] FOREIGN KEY([Record])
	REFERENCES [UFRecords] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFFieldsets] ADD  CONSTRAINT [FK_UFFieldsets_UFPages] FOREIGN KEY([Page])
	REFERENCES [UFPages] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFFields]  ADD  CONSTRAINT [FK_UFField_UFFieldsets] FOREIGN KEY([Fieldset])
	REFERENCES [UFFieldsets] ([Id])
	ON DELETE CASCADE;
	
	
	ALTER TABLE [UFFieldSettings] ADD  CONSTRAINT [FK_UFFieldSettings_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFPrevalues]  ADD  CONSTRAINT [FK_UFPrevalues_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFRecordDataString] ADD  CONSTRAINT [FK_UFRecordDataString_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE;
	
	
	ALTER TABLE [UFRecordDataLongString] ADD  CONSTRAINT [FK_UFRecordDataLongString_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE;
	
	
	
	ALTER TABLE [UFRecordDataInteger] ADD  CONSTRAINT [FK_UFRecordDataInteger_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFRecordDataDateTime]  ADD  CONSTRAINT [FK_UFRecordDataDateTime_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFRecordDataBit] ADD  CONSTRAINT [FK_UFRecordDataBit_UFRecordFields] FOREIGN KEY([Key])
	REFERENCES [UFRecordFields] ([Key])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFFieldConditions] ADD  CONSTRAINT [FK_UFFieldConditions_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFFieldConditionRules]  ADD  CONSTRAINT [FK_UFFieldConditionRules_UFFieldConditions] FOREIGN KEY([FieldCondition])
	REFERENCES [UFFieldConditions] ([Id])
	ON DELETE CASCADE;
	
	ALTER TABLE [UFFieldConditionRules]  ADD  CONSTRAINT [FK_UFFieldConditionRules_UFFields] FOREIGN KEY([Field])
	REFERENCES [UFFields] ([Id]);
	