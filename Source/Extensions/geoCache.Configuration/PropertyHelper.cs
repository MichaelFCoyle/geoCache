//
// File: PropertyHelper.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GeoCache.Configuration
{
	public class PropertyHelper
	{
		private readonly object m_obj;
		private Type m_type;

		public PropertyHelper(object obj) => m_obj = obj ?? throw new ArgumentNullException("obj");

		public IDictionary<string, object> GetProperties(IEnumerable<string> propertyNames)
		{
			if (propertyNames == null)
				throw new ArgumentNullException("propertyNames");

			var dictionary = new Dictionary<string, object>();
			foreach (string propertyName in propertyNames)
			{
				PropertyInfo propertyInfo = GetPropertyInfo(propertyName);
				if (propertyInfo == null)
					continue;
				dictionary.Add(propertyName, propertyInfo.GetValue(m_obj, null));
			}
			return dictionary;
		}

		public void SetProperties(IDictionary<string, object> properties)
		{
			foreach (var keyValue in properties)
				SetProperty(keyValue.Key, keyValue.Value);
		}

		internal void SetProperty(string name, object value)
		{
			if (string.IsNullOrEmpty("name"))
				throw new ArgumentNullException("name");

			PropertyInfo propertyInfo = GetPropertyInfo(name);
			if (propertyInfo == null)
				return;

			Type propertyType = propertyInfo.PropertyType;

			if (value != null && propertyType != value.GetType())
			{
				if (Converter.TryConvert(propertyType, value, out object convertedValue))
					value = convertedValue;
			}
			propertyInfo.SetValue(m_obj, value, null);
		}

		public void WriteProperty(string propertyName, TextWriter writer)
		{
			PropertyInfo propertyInfo = GetPropertyInfo(propertyName);
			if (propertyInfo == null)
				return;

			writer.Write(propertyName + "=");
			object propertyValue = propertyInfo.GetValue(m_obj, null);
			writer.WriteLine(propertyValue);
		}

		private PropertyInfo GetPropertyInfo(string propertyName)
		{
			if (m_type == null)
				m_type = m_obj.GetType();
			
			MemberInfo[] members = m_type.GetMember(propertyName);
			if (members != null && members.Length != 0)
				return members[0] as PropertyInfo;

			//Could not find exact match - Try a case-insensitive search
			foreach (var member in m_type.GetProperties())
				if (string.Equals(member.Name, propertyName, StringComparison.OrdinalIgnoreCase))
					return member;
			
			return null;
		}
	}
}