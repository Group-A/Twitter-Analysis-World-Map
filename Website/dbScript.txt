create table tweet
(tweetIDstr   text(),
analysisVal	double(),
constraint tweet_pk primary key(tweetIDstr));

create table geocode
( tweetIDstr   text(),
coords		text(),
constraint geocode_pk primary key (tweetIDstr,coords),
constraint tweetIDstr_fk1 foreign key (tweetIDstr) references tweet(tweetIDstr));

create table entities
(tweetIDstr   text(),
hashtag		text(),
constraint entities_pk primary key (tweetID_str, hashtag),
constraint tweetIDstr_fk1 foreign key (tweetIDstr) references tweet(tweetIDstr));