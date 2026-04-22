

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

/*#region ================== Namespaces

using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Editing
{
	public class CustomThingsFilter : ThingsFilter
	{

		new public string Name { get { return name; } set { name = value; } }
		new public string CategoryName { get { return categoryname; } set { categoryname = value; } }
		new public int ThingType { get { return thingtype; } set { thingtype = value; } }
		new public ICollection<string> RequiredFields { get { return requiredfields; } }
		new public ICollection<string> ForbiddenFields { get { return forbiddenfields; } }

		// Constructor for a new filter
		public CustomThingsFilter()
		{
			// Initialize
			requiredfields = new List<string>();
			forbiddenfields = new List<string>();
			categoryname = "";
			thingtype = -1;
			name = "Unnamed filter";

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		new public virtual void Dispose()
		{
			base.Dispose();
		}
	}
}*/
