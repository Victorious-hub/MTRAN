# # use CONSTANT => 1;

# # # my %person1 = (
# # #     "name" => 1.56,
# # #     "age"  => 1.2323,
# # #     "city" => "New York"
# # # );
# # # print $person1{"name"} * $person1{"age"};

# # # print 1;

# # # my $age2 = $person1{"name"};

# # # print $age2;

# # # my @a = (1,2,3);

# # # $b = $a[0];
# # # print $b + $a[1] + $a[2];

# # # $adult = 10 + 5;
# # # my $counter123 = 10 + 5;


# # # # print "LOx $adult, $counter123";

# # # # my @list = (2, 20, 30, 40, 50);

# # # # my $resul2t = 0;

# # # # foreach my $num (@list) {
# # # #    print "value of a: $num";
# # # #    $resul2t += $num;
# # # # }
# # # # print $resul2t;
# # # # $h = 3;

# # # # while ($h != 5) {
# # # #     print "value of h: $h";
# # # #     $h++;
# # # # }

# # # # my $age = 1 - 2;

# # # # if ($age <= 18) {
# # # #     print 1;
# # # #     my $counter = 0 + 3;
# # # #     if ($counter == 0) {
# # # #         print 1;
# # # #     } elsif ($counter == 1) {
# # # #         print 2;
# # # #     } else {
# # # #         print 3;
# # # #         my $counter1 = 0 + 3;
# # # #         if ($counter1 == 0) {
# # # #             print 1;
# # # #         } elsif ($counter123 == 1) {
# # # #             print 2;
# # # #         } else {
# # # #             print 3;
# # # #         }
# # # #     }
# # # # } elsif ($age < 65) {
# # # #     print 2;
# # # # } else {
# # # #     print 3;
# # # # }

# # # # sub print_name($age, $counter) {
# # # #     $ee = 2;
# # # #     print "Text";
# # # #     for (my $j = 0; $j < 10; $j++) {
# # # #         print $j;
# # # #         if ($j == 5) {
# # # #             next;
# # # #         }
# # # #         elsif ($j == 6) {
# # # #             last;
# # # #         } else {
# # # #             print "else";
# # # #         }
# # # #         print "PENISSSSS";
# # # #     }
# # # #     return "r";
# # # # }

# # # # my $name1 = print_name(1, 7);
# # # # # my $nameeeeee = $name1;





# # # # my %person = (
# # # #     "name" => "Alice",
# # # #     "age"  => 30,
# # # #     "city" => "New York"
# # # # );

# # # # print $person{"name"}; # prints "Alice"
# # # # print $person{"age"};  # prints 30

# # # # my @fruits = ("apple", "banana", "cherry");

# # # # print $fruits[0]; # prints "apple"
# # # # print $fruits[1]; # prints "banana"

# # # my $scalar = 42;
# # # print $scalar; # prints 42
# # # my $string = "Hello, World!";
# # # print $string; # prints "Hello, World!"


# # # my @numbers = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
# # # foreach my $num (@numbers) {
# # #     if ($num == 3) {
# # #         print "value of a: $num";
# # #         next;
# # #     }

# # #     if ($num == 5) {
# # #         print "value of a: $num. Last";
# # # #         last;
# # # #     }
# # # # }

# # # # my $string1 = "Hello, World!";
# # # # my $string2 = "Hello, World!";

# # # # if ($string1 == $string2) {
# # # #     print "Strings are equal";
# # # # } else {
# # # #     print "Strings are not equal";
# # # # }

# # # # my @arr = (1, 2, 3, 4, 5);

# # sub sum_elems {
# #     my $total = 0;
# #     foreach my $num (@_) {
# #        $total += $num;
# #     }
# #     return "Result: $total";
# # }

# # my $a = sum_elems(1, 2, 3, 4, 5);
# # print $a;

# # my $str3 = 'C:\\Users\\User\\Documents\\file.txt';
# # print $str3; # prints "C:\Users\User\Documents\file.txt"

# # my $str4 = 'Это строка с \'русскими символами\'';
# # print $str4; # prints "Это строка с 'русскими символами'"
# # print (1 + 2) * 3;



# # sub get_value {
# #     my ($hash_ref, $key) = @_;
# #     print $hash_ref;
# #     return $hash_ref->{$key};
# # }

# # my %person = (
# #     "name" => 'Alice',
# #     "age" => 30
# # );
# # my $person_ref = \%person;
# # my $name = get_value(\%person, 'name');
# # print "Name: $name\n";

# # my $ref = \%person;

# # print $person{"name"}; # prints "Alice"
# # # print %ref->{"name"};

# # # ->

# # # for (my $i = 1; $i < 10; $i++) {
# # #     print $i;
# # # }


# # my @numbe1rs = (1,2,3,4,5);
# # # print $numbe1rs[0];
# # my $a33 = 2;
# # for (my $i = 0; $i < @numbe1rs; $i++) {
# #   print "The value of PI is: ", CONSTANT, "\n";
# # }
# # # print "wewe $a33 \n";


# # use constant PI => 3.141592653589793;
# # use constant GREETING => "Hello, Perl!";


# # if (PI > 3) {
# #     print "PI is greater than 3.\n";
# # }

# # my %person = (
# #     "name" => "Alice",
# #     "age"  => 30,
# #     "city" => "New York"
# # );

# # # Цикл for: вычисление квадратов чисел
# # # print "Squares of numbers: ";

# # my @numbers = (1,2,3,4,5,6,7,8,9,10);

# # # foreach my $num (@numbers) {
# # #     print $num**2;  # Выводим квадраты чисел
# # # }

# # my $result1 = 1.7;
# # foreach my $key (@numbers) {
# #     $result1 *= $key;  # Умножаем на каждое число
# #     print $result1;
# # }


# # my $num = -3;
# # print $num;
# # my $n = 5;
# # my $result = 2;
# # for my $i (1 .. $n) {
# #     $result *= $i;
# #     print $result;
# # }

# # $result += 10;

# # print  $result;

# # for (my $i = 0; $i < @numbers; $i++) {
# #     print "Number", $numbers[$i], "\n";

# #     if ($numbers[$i] == 5) {
# #       last;
# #     }
# # }

# # foreach my $num (@numbers) {
# #     print "Number: $num\n";
# #     if ($num == 3) {
# #         next;
# #     }

# #     if ($num == 8) {
# #         last;
# #     }
# # }

# # foreach my $key (keys %person) {
# #     print $key, $person{$key};
# # }




# # print "The value of PI is: ", PI, "\n";         # The value of PI is: 3.141592653589793
# # print "Greeting: ", GREETING, "\n";

# # my $integer = 42;

# # my $float = 3.14;

# # my $negative = -100;

# # my $exp = 1.234e4;   # Это 12300

# # print "$integer\n";     # 42
# # print "$float\n";       # 3.14
# # print "$negative\n";    # -100
# # print "$exp\n";         # 12300


# # my @fruits = (0, 1,2,3);

# # print $fruits[0];
# # print $fruits[1];


# # my $str1 = 'Hello, World!';
# # my $str3 = 'C:\\Users\\User\\Documents\\file.txt';
# # my $str4 = 'Это строка с \'экранированной\' кавычкой.';

# # my @numbers = (1,2,3,4,5,6,7,8,9,10);

# # for (my $i = 0; $i < @numbers; $i++) {
# #     print "Number", $numbers[$i], "\n";

# #     if ($numbers[$i] == 5) {
# #       last;
# #     }
# # }

# # foreach my $num (@numbers) {
# #     print "Number: $num\n";
# #     if ($num == 3) {
# #         next;
# #     }

# #     if ($num == 8) {
# #         last;
# #     }
# # }

# # my $str1 = "apple";
# # my $str2 = "apple";

# # if ($str1 eq $str2) {
# #   print "Строки одинаковые\n";
# # } else {
# #   print "Строки разные\n";
# # }


# my %person1 = (
#     "name" => "Alice",
#     "age"  => 30,
#     "city" => "New York"
# );
# # print $person1{"name"};
# # print $person1{"age"}

# # my $scalar_number = 42;
# # my $scalar_string = "Hello, string";


# sub sum {
#     my $total = 0;
#     foreach my $num (@_) {
#        $total += $num;
#     }
#     return "Result: $total";
# }

# my $result = sum(1, 2, 3, 4, 5);
# print "Sum: $result\n";

# sub factorial {
#     my $n = shift;
#     my $result = 1;
#     for my $i (1 ..$n) {
#         $result *= $i;  # Умножаем на каждое число
#     }
#     return $result;
# }


# my $result = factorial(5);
# print $result;

# package Person;

# sub new {
#     my $class = 1;
#     my $self = {
#         name => shift,
#         age  => shift,
#         city => shift
#     };
#     bless $self, $class;  # Преобразуем хэш в объект
#     return $self;
# }


# sub info {
#     my $self = shift;
#     print "Name", $self->{"name"};
#     print "Age", $self->{"age"};
#     print "City:", $self->{"city"};

# }


# my $person1 = new("Bob", 28, "Chicago");
# # my $name = $person1->info();
# print $person1->info();

# my @people = (
#     new("Alice", 30, "New York"),
#     new("John", 25, "Los Angeles")
# );

# # # Цикл для вывода информации о всех людях
# foreach my $person (@people) {
#     print $person->info();
# }


# use constant GREETING => "Welcome to the Perl world!";
# print GREETING . "ццццццццццццц";




# use strict;
# use warnings;
use strict;
use warnings;

use constant PI => 3.14159;  # Числовая константа

# Текстовая константа
use constant GREETING => "Hello, World!";

# Массив с числами
my @numbers = (1, 2, 3, 4, 5);

# Хэш с данными о человеке
my %person = (
    "name" => "Alice",
    "age"  => 30,
    "city" => "New York"
);

# Цикл for: вычисление квадратов чисел
print "Squares of numbers: ";
for my $num (@numbers) {
    print $num**2 . " ";  # Выводим квадраты чисел
}
print "\n";

# Условие с использованием константы PI
if (PI > 3) {
    print "PI is greater than 3.\n";
}

# Цикл foreach для перебора хэша
print "Person information:\n";
foreach my $key (keys %person) {
    print "$key", $person{$key};
}



sub factorial {
    my $n = shift;
    my $result = 1;
    for my $i (1 ..$n) {
        $result *= $i;  # Умножаем на каждое число
    }
    return $result;
}

# Ввод числа для вычисления факториала
print "Enter a number: ";
my $num = 4;
# chomp($num);

if ($num =~ /^[0-9]+$/) {
    print "The factorial of $num is  " . factorial($num) . "\n";
} else {
    print "Please enter a valid number.\n";
}
print "The factorial of $num  is  " . factorial($num) . "\n";

my $counter = 1;
while ($counter <= 5) {
    print "Counter is: $counter\n";
    $counter++;
}

# Пример использования unless (аналог if)
unless ($num < 0) {
    print "The number is non-negative.\n";
}

package Person;

# Конструктор
sub new {
    my $class = shift;
    my $self = {
        name => shift,
        age  => shift,
        city => shift
    };
    bless $self, $class;  # Преобразуем хэш в объект
    return $self;
}

# Метод для вывода информации о человеке
sub info {
    my $self = shift;
    print "Name:", $self->{"name"};
    print "Age:", $self->{"age"};
    print "City:", $self->{"city"};

}

# Основная программа
package main;

# Создание объекта
$person1 = Person->new("Bob", 28, "Chicago");

# Вывод информации о человеке
# print $person1->info();
$person1->info();

# # Массив с объектами Person
my @people = (
    Person->new("Alice", 30, "New York"),
    Person->new("John", 25, "Los Angeles")
);

# Цикл для вывода информации о всех людях
foreach my $person (@people) {
    $person->info();
    print "----\n";
}

# Строковая константа
use constant GREETING => "Welcome to the Perl world!";
print GREETING . "\n";
