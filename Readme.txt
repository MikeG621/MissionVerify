MissionVerify
===============================================================================
Author: Michael Gaisser (mjgaisser@gmail.com)
Version 2.0
Date: 2018.02.15
===============================================================================

This utility performs simple quality verification steps on X-wing series
mission files.

===================
VERSION HISTORY

v2.0 - 15 Feb 2018
 - Code rewrite
 - Implemented Idmr.Platform
 - Removed Containers, backdrops, etc from AI and Orders checks [YOGEME/#15]
 - AI messages changed "no AI" to "basic AI", downgraded to "Note" [#3]
 - Fixed crash when too many messages generated [#2]
 - Multiple player FlightGroups downgraded to "Warning"

===================
USAGE

The program is primarily intended to be run automatically while saving a
mission in YOGEME. It can also be run manually, with a drag-and-drop region
to drop custom missions from an explorer window.

The following checks and categorizations are currently implemented. More
rigorous checks may be included at a later time.

-=ALL PLATFORMS=-
-Note
 * No in-flight Messages
 * No "Mission Completed" message
 * Basic AI setting

-Warning
 * Multiple player FlightGroups
 * Mission Craft limit exceeded, does not guarantee concurrent existance
 * FlightGroup missing orders
 * Default Briefing duration
 * No Briefing events
 * No Mission description/pre-flight questions.
 
-Failure
 * Mission FlightGroup limit exceeded
 * Mission Message limit exceeded
 * No player FlightGroup

-=TIE=-
-Note
 * No post-mission questions
 
-Failure
 * Logical error in Global Goals guarantees FALSE result
 
-=XWA=-
-Failure
 * Player FlightGroup missing mothership

===============================================================================
Copyright © 2018 Michael Gaisser
This program and related files are licensed under the Mozilla Public License.
See License.txt for the full text. If for some reason the license was not
distributed with this program, you can obtain the full text of the license at
http://mozilla.org/MPL/2.0/.

"Star Wars" and related items are trademarks of LucasFilm Ltd and
LucasArts Entertainment Co.

This software is provided "as is" without warranty of any kind; including that
the software is free of defects, merchantable, fit for a particular purpose or
non-infringing. See the full license text for more details.