#Psmbs quellon
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q "SELECT Name FROM dbo.Psmbs WHERE NormalizedName like '%QUELLON%'"
#EAR Fitoplankton Total 4972
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q "SELECT f.EAR, f.C, a.AssayStart, a.Temperature, p.Name FROM dbo.Psmbs p LEFT OUTER JOIN dbo.PlanktonAssays a ON a.PsmbId = p.Id AND p.NormalizedName like '%QUELLON%' LEFT OUTER JOIN dbo.Phytoplanktons f ON f.PlanktonAssayId = a.Id WHERE p.Id IS NOT NULL AND a.Id IS NOT NULL AND f.EAR IS NOT NULL"
#C Fitoplankton Total 5925
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q "SELECT f.EAR, f.C, a.AssayStart, a.Temperature, p.Name FROM dbo.Psmbs p LEFT OUTER JOIN dbo.PlanktonAssays a ON a.PsmbId = p.Id AND p.NormalizedName like '%QUELLON%' LEFT OUTER JOIN dbo.Phytoplanktons f ON f.PlanktonAssayId = a.Id WHERE p.Id IS NOT NULL AND a.Id IS NOT NULL AND f.C IS NOT NULL"
#Catenella Quellon 14
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q "SELECT f.EAR, f.C, a.AssayStart, a.Temperature, p.Name, s.Name FROM dbo.Psmbs p LEFT OUTER JOIN dbo.PlanktonAssays a ON a.PsmbId = p.Id AND p.NormalizedName like '%QUELLON%' LEFT OUTER JOIN dbo.Phytoplanktons f ON f.PlanktonAssayId = a.Id LEFT OUTER JOIN dbo.SpeciesPhytoplanktons s ON f.SpeciesId = s.Id AND s.NormalizedName like '%CATENELLA%' WHERE p.Id IS NOT NULL AND a.Id IS NOT NULL AND s.Name IS NOT NULL AND f.EAR IS NOT NULL"
#Alexandrium Quellon 10
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q \
"SELECT f.EAR, f.C, a.AssayStart, a.Temperature, p.Name, s.Name \
FROM dbo.Psmbs p \
	LEFT OUTER JOIN dbo.PlanktonAssays a \
		ON a.PsmbId = p.Id \
		AND p.NormalizedName like '%QUELLON%' \
	LEFT OUTER JOIN dbo.Phytoplanktons f \
		ON f.PlanktonAssayId = a.Id \
	LEFT OUTER JOIN dbo.SpeciesPhytoplanktons s \
		ON f.SpeciesId = s.Id \
	LEFT OUTER JOIN dbo.GenusPhytoplanktons g \
		ON s.GenusId = g.Id \
		AND g.NormalizedName like '%Ale%' \
WHERE p.Id IS NOT NULL \
	AND a.Id IS NOT NULL \
	AND f.EAR IS NOT NULL \
	AND s.Id IS NOT NULL \
	AND g.Id IS NOT NULL"

#Alexandrium 56
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q \
"SELECT a.SamplingDate, p.Name, g.Name, s.Name, f.EAR, f.C \
FROM dbo.Psmbs p \
	LEFT OUTER JOIN dbo.PlanktonAssays a \
		ON a.PsmbId = p.Id \
	LEFT OUTER JOIN dbo.Phytoplanktons f \
		ON f.PlanktonAssayId = a.Id \
	LEFT OUTER JOIN dbo.SpeciesPhytoplanktons s \
		ON f.SpeciesId = s.Id \
	LEFT OUTER JOIN dbo.GenusPhytoplanktons g \
		ON s.GenusId = g.Id \
		AND g.NormalizedName like '%Ale%' \
WHERE p.Id IS NOT NULL \
	AND a.Id IS NOT NULL \
	AND f.C IS NOT NULL \
	AND s.Id IS NOT NULL \
	AND g.Id IS NOT NULL"

#Fito Total
sqlcmd -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q \
"SELECT a.SamplingDate, p.C \
FROM dbo.Psmbs p \
	LEFT OUTER JOIN dbo.PlanktonAssays a \
		ON a.PsmbId = p.Id \
	LEFT OUTER JOIN dbo.Phytoplanktons f \
		ON f.PlanktonAssayId = a.Id \
WHERE p.Id IS NOT NULL \
	AND a.Id IS NOT NULL \
	AND f.C IS NOT NULL \
	AND s.Id IS NOT NULL \
	AND g.Id IS NOT NULL"

scp -P 22222 root@45.7.231.195:/root/alexandrium.txt .