# CWEBot : Automated CWE classification of vulnerabilities

The problem of classifying security vulnerabilities using the MITRE CWE categories is very similar in principle to the classic "[20 newsgroups](http://nlp.stanford.edu/wiki/Software/Classifier/20_Newsgroups)" NLP problem of classifying a large number of Usenet posts into a relatively small set of Usenet groups using the text of each post. CWEBot extracts the title and details and other pertinent text information from vulnerabilities data and then uses a NLP toolkit to automatically classify each vulnerability using a model generated from a training and test dataset where the CWE classification for each vulnerability record is known.
## NLP
There are actually many programs and libraries in Java and Python and .NET that do text classification but most of these  appear to use a relatively simple algorithm like Naive Bayesian classification which uses simple frequency statistics on the occurence of words. I imagine they would be similar to how a Bayesian spam classifer works. The industrial-strength NLP toolkits use more adanced algorithms from the theory of statistical natural language processing.

1. [Stanford NLP](http://nlp.stanford.edu/software/). This is the Java framework developed by the Stanford NLP group and from what I understand is usually on the cutting edge of NLP. Christopher Manning who wrote one of the core NLP texts (which is what I'm using right now) is at Stanford and is a key member of this project.

2. [Apache OpenNLP](https://opennlp.apache.org/) This is another Java NLP project sponsored by the Apache project. From what I understand it is at about the same level as Stanford NLP or just a little below, but the key thing is that it uses the more permissive Apache license.

3. [Vowpal Wabbit](https://github.com/JohnLangford/vowpal_wabbit). VW is a general purpose machine learning library that focuses on performance that can also be used for [NLP](https://github.com/hal3/vwnlp/blob/master/GettingStarted.ipynb)

Sincle NLP toolkits can carry less permissive licenses, CWEBot is designed to execute the NLP processing using the unmodified toolkit binaries only, on input data stored as on-disk local files. There is no linking or calling the NLP toolkits from CWEBot at all. Orchestration of the entire pipeline can be orchestrated by shell scripts. 
## Concepts

### Model Dataset
This consists of all vulnerability data that is already CWE-classified. This data is used to create the classification model that will be used on the remaining vulnerability data. The model dataset is subdivided into 2 datasets:
- Training data. This is 20% of the model dataset and is used to train a classification model.
- Test data. This is 80% of the model dataset and is used to test the classification model and report statistics.

For example training data for the 20 newsgroups problem using the Stanford NLP classifier looks like
`alt.atheism	51312	From: bil@okcforum.osrhe.edu (Bill Conner) Subject: Re: Not the Omni! Nntp-Posting-Host: ok...`

The data file is a tab-separated file where column 0 is the class or category that will be inferred, and column i is the the text that will be processed to generate 'features' from which the category of the text will be inferred.

### Target Dataset
This consists of all the vulnerability data that is not CWE classified.

## Stages
### Extract
The extract program or library extracts vulnerabilities from a datasource. The original data can be in any format like JSON or XML or CSV available from any physical or network location like a local file on disk or a REST API accessed over HTTP. The responsibility of the extract stage is to extract all the vulnerabilities data available from the datasource into a single on-disk JSON file with optional compression using a common format that will then be transformed into the  model dataset and target datasets.

There are currently 2 datasources:
- OSS Index: A REST API providing vulnerabilties data in JSON. Most of the vulnerabilities are not yet CWE-classified
- CVE XML: A list of all CVE vulnerabilities in XML in an on-disk GZIP archive. All vulnerability data from this dataset is already classified.

### Transform
The transform program takes the extracted vulnerabilities data in a common JSON format with existing CWE identifiers and transforms it into the model dataset.

#### OSS Index Transfrom
1. The references collection contains a Url like https://cwe.mitre.org/data/definitions/79.html in which case the CWE id is just the relevant segment of the Url.
2. The vulnerability has been assigned a CVE id for which an existing CWE may exist at cwedetails.com in which case we need to fetch the page for the CVE and get the CVE category id.

### Train
One of the supported NLP classifiers is run on the model dataset to generate the model and on the target dataset using the trained model 

### Classify 
One of the supported NLP classifiers is run on the target dataset using the generated model.

## Load
This stage loads the output of the classification stage with the classified vulnerabilities and associated statistics into a desired location and format e.g a CSV file.



