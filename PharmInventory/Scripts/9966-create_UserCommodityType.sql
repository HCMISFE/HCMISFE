CREATE TABLE [dbo].[UserCommodityType](
	[ID] INT IDENTITY(1,1) NOT NULL,
	[UserID] INT NOT NULL,
	[TypeID] INT NOT NULL,
 CONSTRAINT [PK_dbo_UserCommodityType_ID] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [U_dbo_UserCommodityType_UserID_TypeID] UNIQUE NONCLUSTERED 
(
	[UserID] ASC,
	[TypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
 





