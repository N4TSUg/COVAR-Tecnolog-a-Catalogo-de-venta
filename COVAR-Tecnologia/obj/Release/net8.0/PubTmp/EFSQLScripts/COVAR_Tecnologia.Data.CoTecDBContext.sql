IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [Categorias] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [Marcas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Marcas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [ImagenURL] nvarchar(max) NOT NULL,
        [MarcaId] int NOT NULL,
        [CategoriaId] int NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Productos_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(150) NOT NULL,
        [Password] nvarchar(100) NOT NULL,
        [RolId] int NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [TicketsSoporte] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NULL,
        [Asunto] nvarchar(200) NOT NULL,
        [EsComplejo] bit NOT NULL,
        [Estado] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketsSoporte] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketsSoporte_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE TABLE [MensajesSoporte] (
        [Id] int NOT NULL IDENTITY,
        [Texto] nvarchar(max) NOT NULL,
        [FechaEnvio] datetime2 NOT NULL,
        [EsRespuestaAdmin] bit NOT NULL,
        [TicketSoporteId] int NOT NULL,
        CONSTRAINT [PK_MensajesSoporte] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MensajesSoporte_TicketsSoporte_TicketSoporteId] FOREIGN KEY ([TicketSoporteId]) REFERENCES [TicketsSoporte] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [Nombre])
    VALUES (1, N''Administrador''),
    (2, N''Vendedor''),
    (3, N''Cliente'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Password', N'RolId') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [Email], [Password], [RolId])
    VALUES (1, N''enriquearana1402@gmail.com'', N''$2a$12$tzU3g/s8DF2nU3Wu4t5sRuJ0j4jmhmrG0FZBhvadEgNe2Z0PUQnjq'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Password', N'RolId') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE INDEX [IX_MensajesSoporte_TicketSoporteId] ON [MensajesSoporte] ([TicketSoporteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE INDEX [IX_Productos_MarcaId] ON [Productos] ([MarcaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE INDEX [IX_TicketsSoporte_UsuarioId] ON [TicketsSoporte] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511160847_covar1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511160847_covar1', N'9.0.15');
END;

COMMIT;
GO

