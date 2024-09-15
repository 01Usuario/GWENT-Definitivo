using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CompileButton : MonoBehaviour
{
    public TMP_InputField editorInputField; // Cambiado a TMP_InputField

    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(CompileCard);
    }

    void CompileCard()
    {
        string sourceCode = editorInputField.text; // Usar el texto del TMP_InputField
        Lexer lexer = new Lexer(sourceCode);
        List<Token> tokens = lexer.Tokenize();
        Parser parser = new Parser(tokens);
        NodeAST ast = parser.Parse();
        CodeGenerator codeGenerator = new CodeGenerator();
        string generatedCode = codeGenerator.Generate(ast);

        // Crear una instancia de la carta y agregarla a los decks
        CardSO newCard = CardFactory.CreateCardFromAST(ast);
        DeckManager.Instance.AddCardToDecks(newCard);

        // Cargar la escena 1
        SceneManager.LoadScene(1);
    }
}
