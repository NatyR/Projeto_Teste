-- PORTALRH.T_ACESSO definition

-- DDL generated by DBeaver
-- WARNING: It may differ from actual native database DDL
CREATE TABLE PORTALRH.T_ACESSO (
	ID NUMBER NOT NULL,
	SISTEMA_ID NUMBER NOT NULL,
	USUARIO_ID NUMBER NOT NULL,
	IP VARCHAR2(100) NOT NULL,
	URL VARCHAR2(200) NULL,
	METODO VARCHAR2(10) NULL,
	POSTDATA VARCHAR2(4000) NULL,
	DATA_CADASTRO DATE NOT NULL,
	CONSTRAINT T_ACESSO_PK PRIMARY KEY (ID),
	CONSTRAINT FK_T_ACESSO_T_SISTEMA FOREIGN KEY (SISTEMA_ID) REFERENCES PORTALRH.T_SISTEMA(ID),
	CONSTRAINT FK_T_ACESSO_T_USUARIO FOREIGN KEY (USUARIO_ID) REFERENCES PORTALRH.T_USUARIO(ID)
);