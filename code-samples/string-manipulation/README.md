# String Manipulation

Strings can get you into a knot.  They're reference types, but we create them with a literals like a primitive.  They're passed by reference, but the reference could point to the heap or to the intern pool.  In functional terms, they're an array of `char`, but we can't implicitly or explicitly cast between the two.