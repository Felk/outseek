This directory contains classes that contain state for views, so essentially view models.
But the state in these classes is shared across many views. 
Since there usually is a 1:1 relationship between a view and a view model,
this directory exists to clarify that these view models here break that assumption.
They also purposefully break the "XViewModel" naming convention for brevity.
