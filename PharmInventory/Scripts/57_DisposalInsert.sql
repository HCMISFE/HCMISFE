USE [KOLFEHC_DB]
GO
/****** Object:  StoredProcedure [dbo].[proc_DisposalInsert]    Script Date: 1/13/2013 2:26:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[proc_DisposalInsert]
(
	@ID int = NULL output,
	@ItemID int = NULL,
	@StoreId int = NULL,
	@ReasonId int = NULL,
	@Quantity bigint = NULL,
	@Date datetime = NULL,
	@ApprovedBy varchar(50) = NULL,
	@Losses bit = NULL,
	@BatchNo varchar(50) = NULL,
	@Remark text = NULL,
	@Cost float = NULL,
	@RefNo varchar(50) = NULL,
	@EurDate datetime = NULL,
	@RecID int = NULL,
	@UnitID int =null

)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	INSERT
	INTO [Disposal]
	(
		[ItemID],
		[StoreId],
		[ReasonId],
		[Quantity],
		[Date],
		[ApprovedBy],
		[Losses],
		[BatchNo],
		[Remark],
		[Cost],
		[RefNo],
		[EurDate],
		[RecID],
		[UnitID]
		)
	VALUES
	(
		@ItemID,
		@StoreId,
		@ReasonId,
		@Quantity,
		@Date,
		@ApprovedBy,
		@Losses,
		@BatchNo,
		@Remark,
		@Cost,
		@RefNo,
		@EurDate,
		@RecID,
		@UnitID
	)

	SET @Err = @@Error

	SELECT @ID = SCOPE_IDENTITY()

	RETURN @Err
END