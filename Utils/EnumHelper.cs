using JeffPires.BacklogChatGPTAssistant.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace JeffPires.BacklogChatGPTAssistant.Utils
{
    /// <summary>
    /// A helper class for working with enums in C#.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Retrieves the string value of a specified work item type.
        /// If the work item type is a Product Backlog Item, it fetches the value from Azure DevOps.
        /// </summary>
        /// <param name="workItemType">The work item type to retrieve the string value for.</param>
        /// <param name="projectName">The name of the project associated with the work item type.</param>
        /// <returns>The string value of the work item type.</returns>
        public static async Task<string> GetWorkItemTypeStringValueAsync(this WorkItemType workItemType, string projectName)
        {
            if (workItemType == WorkItemType.UserStory)
            {
                return await AzureDevops.GetProjectBacklogItemTypeAsync(projectName);
            }

            return GetStringValue(workItemType);
        }

        /// <summary>
        /// Gets the string value associated with the specified enum item.
        /// </summary>
        /// <param name="enumItem">The enum item.</param>
        /// <returns>The string value associated with the enum item.</returns>
        public static string GetStringValue(this Enum enumItem)
        {
            string enumStringValue = string.Empty;
            Type type = enumItem.GetType();

            FieldInfo objFieldInfo = type.GetField(enumItem.ToString());
            EnumStringValue[] enumStringValues = objFieldInfo.GetCustomAttributes(typeof(EnumStringValue), false) as EnumStringValue[];

            if (enumStringValues.Length > 0)
            {
                enumStringValue = enumStringValues[0].Value;
            }

            return enumStringValue;
        }

        /// <summary>
        /// Gets the enum item associated with the specified string value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="stringValue">The string value associated with the enum item.</param>
        /// <returns>The enum item associated with the specified string value.</returns>
        public static T GetEnumFromStringValue<T>(string stringValue) where T : Enum
        {
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                EnumStringValue attribute = field.GetCustomAttribute<EnumStringValue>();

                if (attribute != null && attribute.Value == stringValue)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException($"No enum value found for string value '{stringValue}' in enum '{typeof(T).Name}'.");
        }
    }

    /// <summary>
    /// Represents an attribute that can be used to associate a string value with an enum value.
    /// </summary>
    public class EnumStringValue : Attribute
    {
        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the EnumStringValue class with the specified value.
        /// </summary>
        /// <param name="value">The value of the EnumStringValue.</param>
        public EnumStringValue(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <returns>The value of the property.</returns>
        public string Value
        {
            get
            {
                return value;
            }
        }
    }
}