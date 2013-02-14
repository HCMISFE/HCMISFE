SELECT isnull(Cost,0) Cost, isnull(QuantityLeft,0) QuantityLeft, ib.*,  (isnull(Cost,0) * QuantityLeft) As Price FROM vwGetReceivedItemsByBatch ib WHERE ib.TypeID = 7 AND ib.StoreId = 8 AND (ib.ExpDate BETWEEN GETDATE() AND GETDATE() + 185 ) AND (ib.Out = 0) ORDER BY Price Desc

select* from vwGetReceivedItemsByBatch where TypeID=7 and StoreID =8 and BatchNo like '%T%'

SELECT isnull(Cost,0) Cost, isnull(QuantityLeft,0) QuantityLeft, ib.*,  (isnull(Cost,0) * QuantityLeft) As Price FROM vwGetReceivedItemsByBatch ib WHERE ib.TypeID = 7 AND ib.StoreId = 8 AND (ib.ExpDate BETWEEN GETDATE() AND GETDATE() + 185 ) AND (ib.Out = 0) ORDER BY Price Desc

alter table Disposal
    drop column NoOfPack, QtyPerPack