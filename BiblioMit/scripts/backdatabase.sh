#Analists
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Analists' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Analists.tsv
#Emails
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Emails' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Emails.tsv
#GenusPhytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.GenusPhytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > GenusPhytoplanktons.tsv
#Laboratories
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Laboratories' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Laboratories.tsv
#Phones
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Phones' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Phones.tsv
#PhylogeneticGroups
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PhylogeneticGroups' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > PhylogeneticGroups.tsv
#Phytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Phytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Phytoplanktons.tsv
#PlanktonAssayEmails
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PlanktonAssayEmails' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > PlanktonAssayEmails.tsv
#PlanktonAssays
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PlanktonAssays' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > PlanktonAssays.tsv
#SamplingEntities
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.SamplingEntities' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > SamplingEntities.tsv
#SpeciesPhytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.SpeciesPhytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > SpeciesPhytoplanktons.tsv
#Stations
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Stations' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" > Stations.tsv
