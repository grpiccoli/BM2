﻿--Comuna.Name,
--EnsayoFitos.Ph,
--Phytoplanktons.C
--FROM EnsayoFitos
--UPDATE EnsayoFitos
--DELETE FROM Origen
--DELETE EnsayoFitos
--DELETE FROM Entries
--EnsayoFitos.Oxigeno,
--DELETE FROM Planilla
--DELETE Phytoplanktons
--EnsayoFitos.Salinidad,
--SELECT MIN(* FROM PSMB
SELECT * FROM EnsayoFito
--Phytoplanktons.Species,
--DELETE FROM EnsayoFitos
--DECLARE @Produccion INT;
--EnsayoFitos.Temperatura,
--SELECT COUNT(*) FROM Planilla
--SELECT COUNT(*) FROM EnsayoFitos
--SELECT EnsayoFitos.FechaMuestreo,
--DELETE FROM Entries WHERE Id = 35
--DELETE FROM Planilla WHERE Data = 4
--DELETE FROM Planilla WHERE Data = 0
--SELECT * FROM Centre WHERE Id = 103438
--SELECT DISTINCT PSMBId FROM EnsayoFitos
--SELECT MAX(FechaMuestreo) FROM EnsayoFitos
--INNER JOIN Comuna ON PSMB.ComunaId = Comuna.Id 
--INNER JOIN PSMB ON EnsayoFitos.PSMBId = PSMB.Id 
--Dato = 5 AND Fecha >= Convert(datetime, '2018-06-01')
--Fecha >= Convert(datetime, '2018-03-01') AND Dato = 4
/*INSERT INTO Planta(Id, CompanyId, ComunaId, Certificable)
--INSERT INTO Columna(Name, ExcelId, Description, Operation)
--VALUES ('TipoItemProduccion', @Produccion, 'Tipo Item', '')
--SELECT Id, Ph FROM EnsayoFitos WHERE Ph IS NOT NULL ORDER BY Ph DESC
--SELECT COUNT(Id) FROM Planilla WHERE Dato = 0 GROUP BY TipoProduccion
--SELECT COUNT(*) FROM Planilla WHERE Dato = 4 AND YEAR(Fecha) = '2017' 
--INSERT INTO @Produccion SELECT Id FROM Excel WHERE Name = 'Producción'
--INNER JOIN Phytoplanktons ON EnsayoFitos.Id = Phytoplanktons.EnsayoFitoId
--DELETE FROM Planilla WHERE Fecha >= Convert(datetime, '2018-01-01') AND Dato = 2
--SELECT * FROM Planilla WHERE Dato = 3 AND Fecha >= Convert(datetime, '2018-09-01')
--(Phytoplanktons.Species LIKE '%Alexandrium%' OR Phytoplanktons.Species LIKE '%Dinophysis%')
--SELECT * FROM Phytoplanktons WHERE Species LIKE '%Alexandrium%' OR Species LIKE '%Dinophysis%'
--SELECT ProductionType, CASE WHEN ProductionType = 4 THEN 0 ELSE ProductionType END AS ProductionType FROM Planilla
VALUES (13099, 76453440, 13116, 0);*/
--SELECT SUM(Peso) FROM Planilla WHERE Dato = 4 AND Fecha >= Convert(datetime, '2018-01-01') AND Fecha < Convert(datetime, '2018-04-01')
--WHERE PSMB.ComunaId IN (10102,10101,10202,10201,10203,10204,10205,10206,10207,10208,10209,10210) AND 
--SET Ph = '7.77' WHERE Id = 291401
--UPDATE EnsayoFitos
--SET Ph = '7.73' WHERE Id = 296511
--UPDATE Excel SET Name = 'Producción' WHERE Id = 3