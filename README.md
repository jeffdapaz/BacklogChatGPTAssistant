# Backlog chatGPT Assistant <img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Microsoft.VisualStudio.Services.Icons.Default" width="5%">

## Description 💬

Backlog chatGPT Assistant is a Visual Studio extension designed to streamline the process of creating backlog items in Azure DevOps using AI. This tool enables users to generate new work items based on pre-selected items, user instructions, or content from text files (like DOCX and PDF) directly within Visual Studio.

### Main Features 🌟

1. **Azure DevOps Integration:** Easily choose where to create new backlog items in Azure DevOps.
2. **AI-Powered Generation:** Utilize AI to generate backlog items based on user instructions or existing documents.
3. **Preview and Edit:** Review and refine generated backlog items before saving them to Azure DevOps.
4. **AI-Enhanced Improvements:** Leverage AI suggestions to enhance backlog items or create child items.

## User Interface 🖥

### Configuration View

In this view, users can select the target Azure DevOps project and provide instructions for generating backlog items:

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_210225.png" width="60%">

### Review and Edit View

This view displays the AI-generated backlog items, allowing users to review, edit, or use AI to further enhance these items or create child tasks:

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_210430.png" width="60%">

You will find this window in menu View -> Other Windows -> Backlog chatGPT Assistant.

## Seamless Workflow ⚙

1. **Select the Source:** Choose an existing work item or upload a document.
2. **Generate Backlog Items:** Use AI to generate detailed backlog items.
3. **Review and Modify:** Edit the AI-generated items as needed.
4. **Persist to Azure DevOps:** Save the final backlog items directly to your Azure DevOps project.

## How It Works 🛠

1. **User Instructions or Documents:** Provide instructions or upload a document containing the information.
2. **AI Processing:** The AI analyzes the content and generates a list of backlog items.
3. **Review and Edit:** You can review, edit, and request further AI-generated suggestions.
4. **Save to Azure DevOps:** Once satisfied, save the items to your selected Azure DevOps project.

## Also Check Out 🔗

### Visual chatGPT Studio <img src="https://user-images.githubusercontent.com/63928228/278760982-5a3be81c-0cb0-4e59-98f6-705b371553e5.png" width="4%"> 

If you find Backlog chatGPT Assistant helpful, you might also be interested in my other extension, [Visual chatGPT Studio](https://marketplace.visualstudio.com/items?itemName=jefferson-pires.VisualChatGPTStudio), which integrates chatGPT directly within Visual Studio to enhance your coding experience with features like intelligent code suggestions, automated code reviews, and more. Ideal for developers who want to boost their productivity and code quality!

## Azure DevOps Authentication and Configuration 🔐

To use the Backlog chatGPT Assistant with Azure DevOps, you need to authenticate and configure your connection. You have two options to set up your credentials:

### 1. Set your PAT (Personal Access Token) in Settings (Optional)
A Personal Access Token (PAT) in Azure DevOps is a security token that allows you to authenticate and access Azure DevOps services. If you prefer, you can configure your PAT directly in the extension's settings:

- **Navigate to the extension settings** within Visual Studio.
- **Locate the PAT configuration option** and input your Personal Access Token.
- **Save the settings**.

### 2. Login Prompt
If you choose not to provide a PAT, you will be prompted to log in with your Azure DevOps credentials when you open the extension. Simply enter your Azure DevOps credentials when prompted.

### Configuring the Azure DevOps URL
In addition to authentication, you'll need to specify the Azure DevOps URL where your work items will be created. This can be done by:

- **Opening the extension settings** within Visual Studio.
- **Entering the Azure DevOps URL** in the designated field.
- **Saving your configuration**.

This URL is essential for establishing the connection between the extension and your Azure DevOps organization or project.


## OpenAI Authentication 🔑

To use this tool it is necessary to connect through the OpenAI API, Azure OpenAI, or any other API that is OpenAI API compatible.

### By OpenAI

1 - Create an account on OpenAI: https://platform.openai.com

2 - Generate a new key: https://platform.openai.com/api-keys

3 - Copy and past the key on options and set the `OpenAI Service` parameter as `OpenAI`: 

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_210708.png" width="55%">

### By Azure

1 - First, you need have access to Azure OpenAI Service. You can see more details [here](https://learn.microsoft.com/en-us/legal/cognitive-services/openai/limited-access?context=%2Fazure%2Fcognitive-services%2Fopenai%2Fcontext%2Fcontext).

2 - Create an Azure OpenAI resource, and set the resource name on options. Example:

<img src="https://github.com/jeffdapaz/VisualChatGPTStudio/assets/63928228/8bf9111b-cc4d-46ac-a4f2-094e83922d95" width="60%">

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_210837.png" width="55%">

3 - Copy and past the key on options and set the `OpenAI Service` parameter as `AzureOpenAI`: 

<img src="https://github.com/jeffdapaz/VisualChatGPTStudio/assets/63928228/2f881df1-a95f-4016-bf39-9cf2e83aef0e" width="75%">

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_210922.png" width="55%">

4 - Create a new deployment through Azure OpenAI Studio, and set the name:

<img src="https://github.com/jeffdapaz/VisualChatGPTStudio/assets/63928228/3914ddf3-e0c5-4edd-9add-dab5aba12aa9" width="40%">

<img src="https://jefferson-pires.gallerycdn.vsassets.io/extensions/jefferson-pires/backlogchatgptassistant/1.0.0/1725050136867/Captura_de_ecr__2024-08-30_211111.png" width="55%">

5 - Set the Azure OpenAI API version. You can check the available versions [here](https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#completions).

### By Others Customs LLM

Is possible to use a service that is not the OpenAI or Azure API, as long as this service is OpenAI API compatible.

This way, you can use APIs that run locally, such as Meta's llama, or any other private deployment (locally or not).

To do this, simply insert the address of these deployments in the `Base API URL` parameter of the extension.

It's worth mentioning that I haven't tested this possibility for myself, so it's a matter of trial and error, but I've already received feedback from people who have successfully doing this.

## Known Issues 🐛

- **Issue 1:** Occasional delays in AI response times.
- **Issue 2:** AI can hallucinate in its responses, generating invalid content.
- **Issue 3:** If the request sent is too long and/or the generated response is too long, the API may cut the response or even not respond at all.
- **Workaround:** Retry generating the backlog items changing the model parameters and/or the instructions.

- **Issue 4:** This extension uses the newly created "Structured Outputs" functionality by the OpenAI (you can see more [here](https://openai.com/index/introducing-structured-outputs-in-the-api)). This feature is not available in all language models or deployments.
- **Workaround:** Make sure you are using a model and a deployment compatible with "Structured Outputs".

## Disclaimer ⚠

- As this extension depends on the API provided by OpenAI or Azure, there may be some change by them that affects the operation of this extension without prior notice.

- As this extension depends on the API provided by OpenAI or Azure, there may be generated responses that not be what the expected.

- The speed and availability of responses directly depend on the API.

- If you are using OpenAI service instead Azure and receive a message like `429 - You exceeded your current quota, please check your plan and billing details.`, check OpenAI Usage page and see if you still have quota, example:

<img src="https://user-images.githubusercontent.com/63928228/242688025-47ec893e-401f-4edb-92a0-127a47a952fe.png" width="60%">

You can check your quota here: [https://platform.openai.com/account/usage](https://platform.openai.com/account/usage)

- If you find any bugs or unexpected behavior, please leave a comment so I can provide a fix.

## Donations 🙏

☕️ If you find this extension useful and want to support its development, consider [buying me a coffee](https://www.paypal.com/donate/?hosted_button_id=2Y55G8YYC6Q3E). Your support is greatly appreciated!

[<img src="https://github-production-user-asset-6210df.s3.amazonaws.com/63928228/278758680-f5fc9df2-a330-4d6a-ae13-9190b7b8f57b.png" width="20%">](https://www.paypal.com/donate/?hosted_button_id=2Y55G8YYC6Q3E)

## Dependencies ⚙

- [OpenAI-API-dotnet](https://github.com/OkGoDoIt/OpenAI-API-dotnet)
- [VsixLogger](https://github.com/madskristensen/VsixLogger)
- [Community.VisualStudio.Toolkit.17](https://github.com/VsixCommunity/Community.VisualStudio.Toolkit)
- [DocX](https://github.com/xceedsoftware/docx)
- [itext7](https://github.com/itext/itext-dotnet)
- [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)

## Release Notes 📜

### 1.0.0

- Initial release with full support for Azure DevOps integration and AI-generated backlog item creation.
