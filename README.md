ABTesting
=========

The ABTesting library is a fixed-up version of the (dufunct?) Fairly Certain library, which was a C-sharp port of the 
Ruby on Rails library A/Bingo [http://www.bingocardcreator.com/abingo].

This library was created by Cole Fichter [www.colefichter.ca] at Mailout Interactive Inc.
(Web: [www.mailoutinteractive.com], GitHub:[https://github.com/organizations/MailoutInteractive])

ABTesting fixes a number of bugs in the Fairly Certain library, and adds support for multi-alternative tests (these are
sometimes called A/B/N or A/B/Z tests).

Installation and Setup
----------------------

Copy the source to your project (or clone the repo). The only configuration required is to point the file path provider(s) at
the files where you'd like to store your data.

The library relies on a rather simple provider model to configure the path on disk to the file store. You'll need to adjust the
hard-coded paths in three different locations.

First, adjust the file path in the class ABTesting.Helpers.FilePathProviders.DebugFilePathProvider. This should point at your
debug (ie: testing or non-production) tests.

Secondly, adjust the file path in the class ABTesting.Helpers.FilePathProviders.ProductionFilePathProvider. This should point at your
production (ie: live, real-world) tests.

Finally, adjust both paths in the class ABTesting.Helpers.FilePathProviders.AutomaticFilePathProvider. You'll also need to adjust
the logic that picks between your debug and production tests.

The usual file extension is ".ab", but this is not required. The library stores the test data as XML in the files you've configured.

Don't worry if the files don't actually exist yet, they'll be created at runtime, if needed.

Two-Alternative Testing
-----------------------

Suppose you want to figure out which of two buttons induces more users to sign up for your service.

### Step 1: Add the test to your page

First, include the controls that you'll use to display the test:

```
<%@ Register TagPrefix='ab' Namespace='ABTesting.Controls' Assembly='ABTesting'  %>
```

Next, add your test controls to the page:

```
<ab:test testname='pub_home_signup_button' runat='server'>		
    <ab:alternative Name='red_button' runat='server' RenderSilently='False'>
        <!-- TODO: replace this comment with the markup for alternative 'red_button' -->
    </ab:alternative>
        <ab:alternative Name='blue_button' runat='server' RenderSilently='False'>
        <!-- TODO: replace this comment with the markup for alternative 'blue_button' -->
    </ab:alternative>
</ab:test>
```

When a user visits the page, he will see one of the alternatives.

### Step 2: Score a conversion

When the user clicks on one of the buttons, you'll need to tell the ABTesting library that a "conversion" needs to be scored.
Basically, this marks the user as having completed the goal action of the test:

```
//In your code-behind button click handler:
//The test name string must match the testname="" attribute listed above!
FairlyCertain.Score("pub_home_signup_button"); 
```

And that's it!

Multiple-Alternative Testing
----------------------------

The usage is the same as that listed above for two-alternative testing, but the library uses different statistical tests to determine
the validity of the test. You can place as many alternatives as you like inside the test.

Viewing Test Results
--------------------

You'll need to build a dashboard to view test results. The following repeater code is an example of how such a
dashboard could be implemented.

'''
<asp:Repeater ID="rpTests" OnItemDataBound="rpTests_ItemDataBound" OnItemCommand="rpTests_ItemCommand" runat="server">
    <ItemTemplate>
        <h2><%# Eval("TestName")%>
            - <em><%# Convert.ToString(Eval("Status")) %></em>
            <asp:LinkButton ID="lnkDelete" runat="server" CommandName="delete" OnClientClick="return confirm('Are you sure you want to delete this test?');"                
                CommandArgument='<%# Eval("TestName") %>'>delete</asp:LinkButton>
        </h2>
        <table class="experiment" cellspacing="0" border="1">
            <tr class="header_row">
                <th>Alternative</th>
                <th align="center">Participants</th>
                <th align="center">Successes</th>
                <th>Notes</th>
            </tr>
            <asp:Repeater ID="rpAlternatives" runat="server">
                <ItemTemplate>
                    <tr class="alternative_row">
                        <td><%# Eval("content")%></td>
                        <td align="center"><%# Eval("participants")%></td>
                        <td align="center"><%# Eval("conversions")%>
                          (<%# Eval("PrettyConversionRate")%>)
                        </td>
                        <td></td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
            <tr class="experiment_row">
                <td><strong>Experiment Total:</strong></td>
                <td align="center"><%# Eval("participants")%></td>
                <td align="center"><%# Eval("conversions")%>
                    (<%# Eval("PrettyConversionRate")%>)
                </td>
                <td></td>
            </tr>
            <tr id="Tr1" runat="server" visible='<%# Eval("IsComplete") %>' style="border-top: 1px solid #DFDEDD;">
                <td colspan="4">
                    <b>Number of Alternatives: </b>
                    <%# ((ABTesting.Experiment)Container.DataItem).Alternatives.Count()%><br />
                    <b>Significance Test: </b>
                    <%# ((ABTesting.Experiment)Container.DataItem).SignificanceTestName%><br />
                    <b>Test Assumptions/Conditions: </b>
                    <%# String.Join("; ", ((ABTesting.Experiment)Container.DataItem).AssumptionsToCheck)%><br />
                    <b>P-Value: </b>
                    <%# ((ABTesting.Experiment)Container.DataItem).GetPValue()%><br />
                    <b>Best Alternative: </b>[<%# ((ABTesting.Experiment)Container.DataItem).GetBestAlternative().Content%>]<br />
                    <%#  (Container.DataItem as ABTesting.Experiment).GetResultDescription()%>
                </td>
            </tr>
        </table>
        <br />
    </ItemTemplate>
</asp:Repeater>
'''


