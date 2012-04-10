ABTesting
=========

The ABTesting library is a fixed-up version of the (dufunct?) Fairly Certain library, which was a C-sharp port of the 
Ruby on Rails library A/Bingo [http://www.bingocardcreator.com/abingo].

ABTesting fixes a number of bugs in the Fairly Certain library, and adds support for multi-alternative tests (these are
sometimes called A/B/N or A/B/Z tests).

Installation and Setup
----------------------

Copy the source to your project (or clone the repo). The only configuration required is to point the file path provider(s) at
the files where you'd like to store your data.

The library relies on a rather simply provider model to configure the path on disk to the file store. You'll need to adjust the
hard-code paths in three different locations.

First, adjust the file path in the class ABTesting.Helpers.FilePathProviders.DebugFilePathProvider. This should point at your
debug (ie: testing or non-production) tests.

Secondly, adjust the file path in the class ABTesting.Helpers.FilePathProviders.ProductionFilePathProvider. This should point at your
production (ie: live, real-world) tests.

Finally, adjust both paths in the class ABTesting.Helpers.FilePathProviders.AutomaticFilePathProvider. You'll also need to adjust
the logic that picks between your debug and production tests.

Don't worry if the files don't actually exist yet, they'll be created at runtime, if needed.

Two-Alternative Testing
-----------------------

Suppose you want to figure out which of the two buttons

