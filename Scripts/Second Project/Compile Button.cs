using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class CompileButton : MonoBehaviour
{
    // Campo de entrada de texto del editor (usando TMP_InputField)
    public TMP_InputField editorInputField;

    void Start()
    {
        // Obtener el componente Button y agregar el listener para el evento de clic
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(CompileCard);
    }

    void CompileCard()
    {
        // Obtener el código fuente del campo de entrada de texto
        string sourceCode = editorInputField.text;

        // Crear una instancia del lexer y tokenizar el código fuente
        Lexer lexer = new Lexer(sourceCode);
        List<Token> tokens = lexer.Tokenize();

        // Crear una instancia del parser y analizar los tokens para generar el AST
        Parser parser = new Parser(tokens);
        List<NodeAST> ast = parser.Parse();

        // Crear instancias de los validadores
        ASTValidator astValidator = new ASTValidator();
        SemanticValidator semanticValidator = new SemanticValidator();

        // Validar cada nodo del AST con ambos validadores
        foreach (var node in ast)
        {
            astValidator.Validate(node);
            semanticValidator.Validate(node);
        }

        // Crear una instancia del generador de código y generar el código
        CodeGenerator codeGenerator = new CodeGenerator(ast);
        codeGenerator.GenerateCode();

        // Cargar la siguiente escena
        SceneManager.LoadScene(1);
    }
}




