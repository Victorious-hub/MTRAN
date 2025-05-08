# use CONSTANT => 1;

# my %person1 = (
#     "name" => 1.56,
#     "age"  => 1.2323,
#     "city" => "New York"
# );
# print $person1{"name"} * $person1{"age"};

# my $age2 = $person1{"name"};

# print $age2;

# my @a = (1,2,3);

# $b = $a[0];
# print $b + $a[1] + $a[2];

# $adult = 10 + 5;
# my $counter123 = 10 + 5;


# print "LOx $adult, $counter123";

# my @list = (2, 20, 30, 40, 50);

# my $resul2t = 0;

# foreach my $num (@list) {
#    print "value of a: $num";
#    $resul2t += $num;
# }
# print $resul2t;
# $h = 3;

# while ($h != 5) {
#     print "value of h: $h";
#     $h++;
# }

# my $age = 1 - 2;

# if ($age <= 18) {
#     print 1;
#     my $counter = 0 + 3;
#     if ($counter == 0) {
#         print 1;
#     } elsif ($counter == 1) {
#         print 2;
#     } else {
#         print 3;
#         my $counter1 = 0 + 3;
#         if ($counter1 == 0) {
#             print 1;
#         } elsif ($counter123 == 1) {
#             print 2;
#         } else {
#             print 3;
#         }
#     }
# } elsif ($age < 65) {
#     print 2;
# } else {
#     print 3;
# }

# sub print_name($age, $counter) {
#     $ee = 2;
#     print "Text";
#     for (my $j = 0; $j < 10; $j++) {
#         print $j;
#         if ($j == 5) {
#             next;
#         }
#         elsif ($j == 6) {
#             last;
#         } else {
#             print "else";
#         }
#         print "PENISSSSS";
#     }
#     return "r";
# }

# my $name1 = print_name(1, 7);
# # my $nameeeeee = $name1;





# my %person = (
#     "name" => "Alice",
#     "age"  => 30,
#     "city" => "New York"
# );

# print $person{"name"}; # prints "Alice"
# print $person{"age"};  # prints 30

# my @fruits = ("apple", "banana", "cherry");

# print $fruits[0]; # prints "apple"
# print $fruits[1]; # prints "banana"

# my $scalar = 42;
# print $scalar; # prints 42
# my $string = "Hello, World!";
# print $string; # prints "Hello, World!"


# my @numbers = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
# foreach my $num (@numbers) {
#     if ($num == 3) {
#         print "value of a: $num";
#         next;
#     }

#     if ($num == 5) {
#         print "value of a: $num. Last";
#         last;
#     }
# }

# my $string1 = "Hello, World!";
# my $string2 = "Hello, World!";

# if ($string1 == $string2) {
#     print "Strings are equal";
# } else {
#     print "Strings are not equal";
# }

# my @arr = (1, 2, 3, 4, 5);

# sub sum_elems {
#     my $total = 0;
#     foreach my $num (@_) {
#        $total += $num;
#     }
#     return "Result: $total";
# }

# a = sum_elems(1, 2, 3, 4, 5);

# my $str3 = 'C:\\Users\\User\\Documents\\file.txt';
# print $str3; # prints "C:\Users\User\Documents\file.txt"

# my $str4 = 'Это строка с \'русскими символами\'';
# print $str4; # prints "Это строка с 'русскими символами'"


# my %person = (
#     "name" => "Alice",
#     "age"  => 30,
#     "city" => "New York"
# );

# print (1 + 2) * 3;



sub get_value {
    my ($hash_ref, $key) = @_;
    # return $hash_ref->{$key};
}

my %person = (
    "name" => 'Alice',
    "age" => 30
);
my $person_ref = \%person;
my $name = get_value($person_ref, 'name');
print "Name: $name\n";

# my $ref = \%person;

# print $ref->{"name"}; # prints "Alice"

# ->
