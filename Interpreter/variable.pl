use CONSTANT => 1;

my %person = (
    "name" => 1.56,
    "age"  => 1.2323,
    "city" => "New York"
);
print $person{"name"} * $person{"age"};

my $age2 = $person{"name"};

print $age2;

my @a = (1,2,3);

$b = $a[0];
print $b + $a[1] + $a[2];

$adult = 10 + 5;
my $counter123 = 10 + 5;


print "LOx $adult, $counter123";

my @list = (2, 20, 30, 40, 50);

my $result = 0;

foreach my $num (@list) {
   print "value of a: $num";
   $result += $num;
}
print $result;
$h = 3;

while ($h != 5) {
    print "value of h: $h";
    $h++;
}


# # my %person = (
# #     'name' => "Bob",
# #     age'  => 1.2323,
# #     city => "New York"
# # );

my $age = 1 - 2;

if ($age <= 18) {
    print 1;
    my $counter = 0 + 3;
    if ($counter == 0) {
        print 1;
    } elsif ($counter == 1) {
        print 2;
    } else {
        print 3;
        my $counter1 = 0 + 3;
        if ($counter1 == 0) {
            print 1;
        } elsif ($counter123 == 1) {
            print 2;
        } else {
            print 3;
        }
    }
} elsif ($age < 65) {
    print 2;
} else {
    print 3;
}

# for (my $j = 1; $j <= 3; $j++) {
#     $age++;
# }


sub print_name($age, $counter) {
    $ee = 2;
    print "Text";
    return "r";
}

my $name1 = print_name(1, 7);
my $nameeeeee = $name1;
