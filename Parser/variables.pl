sub check_age($age) {
    if ($age < 18) {
        for (my $i = 1; $i <= 5; $i++) {
            my $name = "Alice";
        }
        return "young";
    } elsif ($age < 65) {
        return "adult";
    } else {
        return "pensioner";
    }
}

my $age = check_age(10);

print "Hello";


sub student_data
{
 
    # shift will take package name 'student' 
    # and assign it to variable 'class'
    my $class_name = shift;
    
  
    # returning object from constructor
    return $self;
}
 
# Object creating and constructor calling
# my $Data = new student_data student("Geeks","forGeeks");


package Person;


sub func {
   my $class = shift;
   my $self = (
      _firstName => shift,
      _lastName  => shift,
      _ssn       => shift,
   );
   # Print all the values just for clarification.
   print "First Name is";
   print "Last Name is";
   print "SSN is";
   bless $self, $class;
   return $self;
}


my $c = 1 * 22 + 10;
my $name = "Alice";


if ($age <= 18) {
    my $c = 1 * 22 + 10;
} elsif ($age < 65) {
    my $c = 1 * 22 + 10;
} else {
    my $c = 1 * 22 + 10;
}

# while (10 == 9 && 1 ^ 2 or 2 != 3) {
#     # my $c = 1 * 22 + 10;
#     $counter++;
# }

# # my $name = "Alice";

# # $a = 10 + 5;
# # my @colors = ("Red", "Green", "Blue");

sub check_age {
    # my ($age) = @_;
    if ($age < 18) {
        for (my $i = 1; $i <= 5; $i++) {
            my $name = "Alice";
        }
        return "young";
    } elsif ($age < 65) {
        return "adult";
    } else {
        return "pensioner";
    }
}

$e = 1 * (2+4);


my %person = (
    name => "Bob",
    age  => 1.2323,
    city => "New York"
);

my @colors = ("Red", "Green", "Blue");


while (10 == 9 && 1 ^ 2 or 2 != 3) {
    # my $c = 1 * 22 + 10;
    $counter++;
}


foreach my $num (@numbers) {
    $counter++;
}


for (my $i = 1; $i <= 3; $i++) {
    for (my $j = 1; $j <= 3; $j++) {
        $counter++;
    }
}
my $a += 1;
$a += 1;

my $a = 1;

my $adult = 10 + $a;


while (10 == 9 && 1 ^ 2 or 2 != 3) {
    # my $c = 1 * 22 + 10;
    $counter++;
}

for (my $j = 1; $j <= 3; $j++) {
        $counter++;
    }

# $j <= 3;
my $j = 1;

$j = 1;