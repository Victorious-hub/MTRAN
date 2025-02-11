my <1> = "Alice";

sub <6> {
    my (<10>) = <13>;
    
    if (<10> < 18) {
        return "Несовершеннолетний";
    } elsif (<10> < 65) {
        return "Взрослый";
    } else {
        return "Пенсионер";
    }
}

for (my <47> = 1; <47> <= 5; <47>++) {
    print "some text";
}

my <64> = ("Red", "Green", "Blue");
print <64>[0];
foreach my <82> (<64>) {
    print "hello world";
}

my <92> = (
    <95> => "Bob",
    <99>  => 30,
    <103> => "New York"
);


my <109> = 0;
while (<109> < 3) {
    print "Счётчик";
    <109>++;
}
my <128> = <6>(25);


my $1var = "test";

print 1.232.3.1 ;
my <144> = 100 ¥ 20;

print "w
