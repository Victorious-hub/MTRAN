$adult = 10 + 5;
my $counter123 = 10 + 5;

# my %person = (
#     'name' => "Bob",
#     age'  => 1.2323,
#     city => "New York"
# );

my $age = 1 ^ 2;

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

for (my $j = 1; $j <= 3; $j++) {
    $age++;
}

my @numbers = ("Red", "Green", "Blue");
foreach my $num (@numbers) {
    @numbers++;
}


sub print_name($age, $counter) {
    my $name = "Hello";
    print "Text";
    return $name;
}

my $name1 = print_name(1, 7, "e");

while (10 == 9 && 1 ^ 2) {
    my $c = 1 * 22 + 10;
    $age++;
}

# print $name;

package student;

sub func23 {
   my $class = shift;
   my $self = {
      _firstName => shift,
      _lastName  => shift,
      _ssn       => shift,
   };

   bless $self, $class;
   return $self;
}

my %person = (
    "name" => "Bob",
    "age"  => 1.2323,
    "city" => "New York"
);

# my $age = 1 ^ 2 + (1;

# my age = 1 ^ 2 + (1;