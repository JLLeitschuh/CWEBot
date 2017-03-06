@echo off
cwb transform .\data\nvd.all.json.gz -f nvd.all -s 1 --overwrite & cwb train nvd.all.training.tsv nvd.all.test.tsv nvd.44000.model.gz