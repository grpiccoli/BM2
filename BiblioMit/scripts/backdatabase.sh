#Analists
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Analists' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Analists.tsv
#Emails
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Emails' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Emails.tsv
#GenusPhytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.GenusPhytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > GenusPhytoplanktons.tsv
#Laboratories
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Laboratories' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Laboratories.tsv
#Phones
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Phones' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Phones.tsv
#PhylogeneticGroups
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PhylogeneticGroups' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > PhylogeneticGroups.tsv
#Phytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Phytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Phytoplanktons.tsv
#PlanktonAssayEmails
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PlanktonAssayEmails' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > PlanktonAssayEmails.tsv
#PlanktonAssays
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PlanktonAssays' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > PlanktonAssays.tsv
#PlanktonUsers
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.PlanktonUsers' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > PlanktonUsers.tsv
#SamplingEntities
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.SamplingEntities' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > SamplingEntities.tsv
#SpeciesPhytoplanktons
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.SpeciesPhytoplanktons' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > SpeciesPhytoplanktons.tsv
#Stations
sqlcmd -h -1 -s "^" -W -k 1 -r 1 -S localhost -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-3E10FA62-82AF-4FA8-91A7-71A1040A7646 -Q 'SELECT * FROM dbo.Stations' | tr '^' '\t' | grep '^[1-9]' | sed "s/NULL//g" | awk 'sub("$", "\r")' | iconv -f utf-8 -t utf-16 - > Stations.tsv
