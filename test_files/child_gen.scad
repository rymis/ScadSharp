module m1() { cube(10, center = true); }
module m2() { sphere(7); }
module e();

module c1() {
    children(0);
}

c1() {
    m1();
    m2();
}
